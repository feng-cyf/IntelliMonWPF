import asyncio
import multiprocessing
from concurrent.futures import ProcessPoolExecutor, wait
from typing import Optional
from fastapi import APIRouter, HTTPException
import numpy as np
from pydantic import BaseModel, field_validator
from datetime import datetime
from typing import List

app_data = APIRouter(tags=["DataSaveApi"], prefix="/data")

global_process_pool = ProcessPoolExecutor(max_workers=multiprocessing.cpu_count())


class Data(BaseModel):
    DeviceName: str
    SlaveId: int
    Time: Optional[datetime] = None
    ushorts: Optional[List[float]] = None

    @field_validator("Time")
    def time_validator(cls, v):
        if isinstance(v, str):
            return datetime.fromisoformat(v)
        return v


class Request(BaseModel):
    code: int
    message: str
    Number: int
    Sum: Optional[float] = None
    Mean: Optional[float] = None
    Max: Optional[float] = None
    Min: Optional[float] = None
    Std: Optional[float] = None
    Median: Optional[float] = None
    PeakToPeak: Optional[float] = None
    Energy: Optional[float] = None  # 平方和


def perform_calculations(data_list):
    """在进程池中执行的计算函数"""
    try:
        arr = np.array(data_list, dtype=np.float64)

        return {
            "code": 200,
            "message": "计算成功",
            "Number": len(arr),
            "Sum": float(np.sum(arr)),
            "Mean": round(float(np.mean(arr)), 4),
            "Max": float(np.max(arr)),
            "Min": float(np.min(arr)),
            "Std": round(float(np.std(arr)), 4),
            "Median": float(np.median(arr)),
            "PeakToPeak": float(np.ptp(arr)),
            "Energy": float(np.sum(arr ** 2))
        }
    except Exception as e:
        return {
            "code": 500,
            "message": f"计算失败: {str(e)}",
            "Number": 0
        }


@app_data.post("/data", response_model=Request)
async def data(request: Data):
    """处理数据并返回计算结果"""
    if request.ushorts is None or len(request.ushorts) == 0:
        raise HTTPException(status_code=400, detail="传入结果为空")

    try:
        #数据量过多，并发太多，可以使用任务队列配合有限的进程池导入，使用http轮询获取信息以及状态
        future = global_process_pool.submit(perform_calculations, request.ushorts)

        result = await asyncio.to_thread(future.result)

        return result

    except Exception as e:
        return {
            "code": 500,
            "message": f"服务器错误: {str(e)}",
            "Number": 0
        }


@app_data.on_event("shutdown")
def shutdown_event():
    """应用关闭时关闭进程池"""
    global_process_pool.shutdown()
    print("Process pool has been shut down")