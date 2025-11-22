import uvicorn
from tortoise.contrib.fastapi import register_tortoise
from fastapi import FastAPI

from Helper.loggingHelper import setup_logging
from orm.api.DataSaveApi import app_data
from orm.api.DataToCE import app_pandas
from orm.api.DeviceDataSaveApi import app_dataSave
from orm.api.LoginApi import api_login
from orm.api.RegisterApi import api_register
from orm.api.SelectApi import app_Select
from orm.dict import TORTOISE_ORM
from DataBaseControl.redisClient import RedisClient
app = FastAPI()
logg=setup_logging()
@app.on_event("startup")
async def startup():
    try:
        r = RedisClient()
        await r.init()
        app.state.redis=await r.get_redis()
        logg.info("Redis init success")
    except Exception as ex:
        logg.error(ex)
app.include_router(api_login)
app.include_router(api_register)
app.include_router(app_Select)
app.include_router(app_dataSave)
app.include_router(app_data)
app.include_router(app_pandas)
register_tortoise(app=app,config=TORTOISE_ORM,generate_schemas=True,
    add_exception_handlers=True)
if __name__ == "__main__":
    uvicorn.run(app,loop="asyncio")