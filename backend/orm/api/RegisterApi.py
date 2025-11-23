from datetime import datetime, date  # 新增 date 导入（按需选择日期/时间类型）
from typing import Optional

from fastapi import APIRouter
from pydantic import BaseModel, Field
from tortoise.exceptions import ValidationError  # 导入具体异常，精准捕获

from orm.model import User, Job  # 确保 User/Job 模型导入正确

api_register = APIRouter(tags=["register"], prefix="/register")


class RegisterJob(BaseModel):
    name: str
    role: str
    timestamp: Optional[date] = Field(default_factory=date.today)  # 推荐用 date（避免时间格式问题）

    class Config:
        from_attributes = True
        json_encoders = {
            date: lambda d: d.strftime("%Y-%m-%d"),
            datetime: lambda dt: dt.strftime("%Y-%m-%d %H:%M:%S")  # 若用 datetime 则保留
        }


class RegisterUser(BaseModel):
    username: str
    password: str  # 客户端传入的明文密码（需加密后存 pwd_hash）
    timestamp: Optional[date] = Field(default_factory=date.today)
    job_id: int


class EndMessage(BaseModel):
    code: int
    message: str

    class Config:
        from_attributes = True

@api_register.post("/job", response_model=EndMessage)
async def register_job(job: RegisterJob):
    try:
        new_job = await Job.create(
            name=job.name,
            role=job.role,
            timestamp=job.timestamp  
        )


        print(f"创建的职位：{new_job.name}，时间：{new_job.timestamp}")

        if new_job:
            return EndMessage(code=200, message=f"职位「{new_job.name}」注册成功")

    except ValidationError as e:
        error_msg = f"参数错误：{str(e)}"
        print(f"错误：{error_msg}")
        return EndMessage(code=400, message=error_msg)
    except Exception as e:
        error_msg = f"保存出现问题：{str(e)}"
        print(f"错误：{error_msg}")
        return EndMessage(code=500, message=error_msg)


@api_register.post("/user", response_model=EndMessage)
async def register_user(user: RegisterUser):
    try:
        try:
            job = await Job.get(id=user.job_id)  # 检查 Job 是否存在
        except Job.DoesNotExist:
            return EndMessage(code=400, message=f"职位 ID {user.job_id} 不存在")

        import bcrypt
        def hash_password(plain_pwd: str) -> str:
            salt = bcrypt.gensalt()
            return bcrypt.hashpw(plain_pwd.encode("utf-8"), salt).decode("utf-8")

        new_user = await User.create(
            username=user.username,
            pwd_hash=hash_password(user.password),
            job_id=user.job_id,
            timestamp=user.timestamp
        )

        return EndMessage(code=200, message=f"用户「{new_user.username}」注册成功")

    except ValidationError as e:
        error_msg = f"参数错误：{str(e)}"
        print(f"错误：{error_msg}")
        return EndMessage(code=400, message=error_msg)
    except Exception as e:
        error_msg = f"保存出现问题：{str(e)}"
        print(f"错误：{error_msg}")
        return EndMessage(code=500, message=error_msg)