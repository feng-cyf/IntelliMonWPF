from typing import Optional, Union, List

from fastapi import APIRouter, HTTPException
from pydantic import BaseModel, validator, field_validator
from tortoise.exceptions import DoesNotExist
from tortoise.expressions import Q

from orm.model import Device, PointConfig, DataSave, Prediction
from orm.model import (
    DataSave_Pydantic, Prediction_Pydantic, PredictionIn_Pydantic,
    DataSaveIn_Pydantic, PointConfig_Pydantic, PointConfigIn_Pydantic,
    Device_Pydantic, DeviceIn_Pydantic
)

app_dataSave = APIRouter(tags=["DeviceDataSave"], prefix="/deviceSave")

class PointRequestItem(PointConfigIn_Pydantic):
    DeviceName: str
    SlaveId: int

class Response(BaseModel):
    code: int
    message: str
    data: dict | str | None = None


class DeviceDataSaveRequest(BaseModel):
    devices: List[DeviceIn_Pydantic]

    @field_validator('devices')
    def check_devices_not_empty(cls, v):
        if not v:
            raise ValueError('设备列表不能为空')
        return v
class PointSaveRequest(BaseModel):
    points: List[PointRequestItem]


@app_dataSave.post("/data/batch", response_model=Response)
async def batch_save_devices(request: DeviceDataSaveRequest):
    """批量保存设备数据到数据库，统一一响应数据结构"""
    saved_devices = []
    duplicate_devices = []
    error_info = None

    try:
        for device_data in request.devices:
            try:
                existing_device = await Device.get(
                    DeviceName=device_data.DeviceName,
                    SlaveId=device_data.SlaveId
                )
                duplicate_devices.append({
                    "DeviceName": device_data.DeviceName,
                    "SlaveId": device_data.SlaveId,
                    "message": "设备已存在"
                })
                continue
            except Exception:
                pass

            try:
                device = await Device.create(**device_data.dict())
                saved_device = await Device_Pydantic.from_tortoise_orm(device)
                saved_devices.append(saved_device)
            except Exception as e:
                error_info = f"保存设备 {device_data.DeviceName} 失败: {str(e)}"
                return {
                    "code": 500,
                    "message": error_info,
                    "data": {
                        "saved": saved_devices if saved_devices else None,
                        "duplicates": duplicate_devices if duplicate_devices else None,
                        "error": error_info
                    }
                }

        if duplicate_devices:
            message = f"成功保存 {len(saved_devices)} 个设备，发现 {len(duplicate_devices)} 个重复设备"
            return {
                "code": 207,  # 部分成功状态码
                "message": message,
                "data": {
                    "saved": saved_devices if saved_devices else None,
                    "duplicates": duplicate_devices,
                    "error": None
                }
            }
        else:
            return {
                "code": 200,
                "message": f"所有 {len(saved_devices)} 个设备已成功保存",
                "data": {
                    "saved": saved_devices,
                    "duplicates": None,
                    "error": None
                }
            }

    except Exception as e:
        error_info = f"批量保存失败: {str(e)}"
        return {
            "code": 500,
            "message": error_info,
            "data": {
                "saved": saved_devices if saved_devices else None,
                "duplicates": duplicate_devices if duplicate_devices else None,
                "error": error_info
            }
        }
@app_dataSave.post("/data/pointSave", response_model=Response)
async def batch_save_points(request: PointSaveRequest):
    point_save_success = []
    point_save_failure = []
    device_none=[]
    for point in request.points:
        device=None
        try:
            device = await Device.get(DeviceName=point.DeviceName,SlaveId=point.SlaveId)
        except DoesNotExist:
            device_none.append({"message":"当前设备不存在","DeviceName":point.DeviceName,"SlaveId":point.SlaveId})
        try:
             try:
                 await PointConfig.get(PointName=point.PointName,device=device)
                 point_save_failure.append({"message":"重复点名","pointName":point.PointName})
             except DoesNotExist:
                 await PointConfig.create(device=device,**point.dict(exclude={"DeviceName", "SlaveId"}))
                 point_save_success.append({"pointName":point.PointName,"message":"保存成功"})
        except Exception as e:
            point_save_failure.append({"pointName":point.PointName,"message":str(e)})
    return {"code":200,"message":f"保存成功{len(point_save_success)}个点名,保存失败{len(point_save_failure)}个点名,不存在设备{len(device_none)}个"
            ,"data":{"point_save_success":point_save_success,"point_save_failure":point_save_failure,"device_none":device_none}}
class EditPoint(BaseModel):
    DeviceName: str
    SlaveId:int
    PointName: str
    AccessType:str
    Unit:str
    ScaleFactor:str
    Offset:str
    Desc:str
    class Config:
        from_attributes = True
class EditPointConfig(BaseModel):
    EditPointList: List[EditPoint]


@app_dataSave.post("/data/editPoint", response_model=Response)
async def edit_point(ep: EditPointConfig):
    device_ds = [(r.DeviceName, r.SlaveId) for r in ep.EditPointList]
    conditions = Q()
    for name, sid in device_ds:
        conditions |= Q(DeviceName=name) & Q(SlaveId=sid)

    devices = await Device.filter(conditions)
    if not devices:
        return {
            "code": 200,
            "message": "未找到设备",
            "data": {"existing_points": []}
        }
    device_ids=[d.id for d in devices]
    device_map = {(r.DeviceName, r.SlaveId): r for r in devices}

    point_names = [p.PointName for p in ep.EditPointList]

    existing_points = await PointConfig.filter(
        device_id__in=device_ids,
        PointName__in=point_names
    ).select_related("device")

    existing_points_map = {(p.device.id, p.PointName): p for p in existing_points}

    to_update = []
    for item in ep.EditPointList:
        device = device_map.get((item.DeviceName, item.SlaveId))
        if not device:
            continue

        key = (device.id, item.PointName)
        if key not in existing_points_map:
            continue

        # 更新属性
        point_obj = existing_points_map[key]
        point_obj.AccessType = item.AccessType
        point_obj.Unit = item.Unit
        point_obj.ScaleFactor = item.ScaleFactor
        point_obj.Offset = item.Offset
        point_obj.Desc = item.Desc

        to_update.append(point_obj)
    # 判断列表不为空再执行批量更新
    if to_update:
        await PointConfig.bulk_update(
            to_update,
            fields=['AccessType', 'Unit', 'ScaleFactor', 'Offset', 'Desc']
        )

    return {
        "code": "200",
        "message": f"更新 {len(to_update)} 条数据",
        "data": {
            "existing_points": [f"{p.device.DeviceName}:{p.PointName}" for p in to_update]
        }
    }
