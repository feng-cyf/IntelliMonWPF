from fastapi import APIRouter, HTTPException, status
from pydantic import BaseModel
from tortoise.exceptions import DoesNotExist
import bcrypt
from orm.model import User, Job

api_login = APIRouter(tags=["login"], prefix="/login")

class LoginRequest(BaseModel):
    username: str
    password: str
class LoginSuccessResponse(BaseModel):
    code: int = status.HTTP_200_OK
    message: str = "登录成功"
    data: dict

    class Config:
        orm_mode = True

@api_login.post("/", response_model=LoginSuccessResponse)
async def login(login_data: LoginRequest):
    """
    员工登录接口
    - 输入：用户名（username）、明文密码（password）
    - 输出：登录结果 + 用户基本信息（含职位信息）
    """
    try:
        user = await User.get(username=login_data.username).prefetch_related("job")
    except DoesNotExist:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="用户名不存在，请检查输入"
        )

    plain_password = login_data.password.encode("utf-8")
    db_pwd_hash = user.pwd_hash.encode("utf-8")

    if not bcrypt.checkpw(plain_password, db_pwd_hash):
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="密码错误，请重新输入"
        )

    user_info = {
        "user_id": user.id,
        "username": user.username,
        "job": {
            "job_id": user.job.id,
            "job_name": user.job.name,
            "job_role": user.job.role
        },
        "create_time": user.timestamp
    }
    return {
        "code": status.HTTP_200_OK,
        "message": "登录成功",
        "data": user_info
    }
