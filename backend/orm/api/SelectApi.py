from typing import List, Optional

from fastapi import APIRouter
from pydantic import BaseModel, ConfigDict

from orm.model import Job  # 导入ORM模型

app_Select = APIRouter(tags=["select"], prefix="/select")


class JobSchema(BaseModel):
    id: int
    name: str
    role: str

    model_config = ConfigDict(from_attributes=True)  # 允许从ORM实例创建


# 2. 定义API响应模型
class SelectJobResponse(BaseModel):
    Message: str
    code: int
    data: List[JobSchema]


@app_Select.get("/", response_model=SelectJobResponse)
async def select_job():
    jobs = await Job.all()

    if jobs:
        return {"code":200,"Message": "查询成功", "data": jobs}
    else:
        return {"code":-99,"Message": "查询失败", "data": []}
