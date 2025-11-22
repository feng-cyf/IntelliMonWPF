from fastapi import APIRouter, Request
import asyncio, uuid, os, logging
import pandas as pd
from datetime import datetime
from typing import List, Optional, Union
from pydantic import BaseModel, root_validator

from orm.model import PointConfigIn_Pydantic
from Helper.ExcelHelper import auto_adjust_column_width
from tasks.thread_pool_trigger import ImportThread

app_pandas = APIRouter(prefix="/pandas", tags=["Csv_Excel"])

class Response(BaseModel):
    code: int
    message: Union[str, dict]
    data: Union[str,dict] = None

class BasePointConfig(PointConfigIn_Pydantic, BaseModel):
    class Config:
        extra = "allow"

class PandasData(BaseModel):
    l: List[BasePointConfig]
    part: str
    name: Optional[str] = None
    Type: str

    @root_validator(pre=True)
    def set_name_if_empty(cls, values):
        if not values.get("name"):  # 为空或None
            values["name"] = datetime.now().strftime("%Y-%m-%d_%H_%M_%S")
        return values
async def save_file_async(data: PandasData, u: uuid.UUID, redis):
    try:
        def write_file():
            os.makedirs(data.part, exist_ok=True)
            ext = "csv" if data.Type == "Csv" else "xlsx"
            file_path = os.path.join(data.part, f"{data.name}.{ext}")

            df = pd.DataFrame([row.dict() for row in data.l])
            if ext == "csv":
                df.to_csv(file_path, index=False, encoding="utf-8-sig")
            else:
                df.to_excel(file_path, index=False,engine="openpyxl")
                auto_adjust_column_width(file_path)
            return file_path

        file_path = await asyncio.to_thread(write_file)
        await redis.hset(f"pandas:{u}", "status", "success")
        logging.info(f"[后台任务完成] 文件已生成: {file_path}")
    except Exception as e:
        await redis.hset(f"pandas:{u}", "status", "failed")
        logging.warning(f"[后台任务失败] 保存文件出错: {str(e)}")

@app_pandas.post("/csv_excel", response_model=Response)
async def pandas_csv(data: PandasData, request: Request):
    redis = request.app.state.redis
    u = uuid.uuid1()
    await redis.hset(f"pandas:{u}", mapping={
        "part": data.part,
        "name": data.name,
        "Type": data.Type,
        "status": "waiting"
    })
    await redis.expire(f"pandas:{u}", 86400)
    asyncio.create_task(save_file_async(data, u, redis))
    logging.info(f"Excel保存已提交,任务id为{u}")
    return {"code": 200, "message": f"{data.part}/{data.name} 正在生成", "data": str(u)}

@app_pandas.post("/csv_excel/{u}", response_model=Response)
async def pandas_excel(u: uuid.UUID, request: Request):
    redis = request.app.state.redis
    data = await redis.hgetall(f"pandas:{u}")
    if not data:
        return {"code": 404, "message": "任务不存在", "data": None}
    status = data["status"]
    del data["status"]
    return {"code": 200, "message": f"{status}", "data": data}