from tortoise.contrib.pydantic import pydantic_model_creator
from tortoise.models import Model
from tortoise import fields
class Job(Model):
    id = fields.IntField(pk=True)
    name = fields.CharField(unique=False, max_length=20, null=False)
    role = fields.CharField(unique=False, max_length=20, null=False)
    timestamp = fields.DateField(null=False)
class User(Model):
    id = fields.IntField(pk=True)
    username = fields.CharField(unique=False, max_length=20, null=False)
    pwd_hash = fields.CharField(null=False,max_length=150)
    class Meta:
        unique_together = ("username", "pwd_hash")
    job = fields.ForeignKeyField(
        "models.Job",
        related_name="employees",
        null=False
    )
    timestamp = fields.DateField(null=False)


class Device(Model):
    id = fields.IntField(pk=True)
    DeviceName = fields.CharField(unique=False, max_length=60, null=False)
    SlaveId = fields.IntField(unique=False, max_length=10, null=False)
    class Meta:
        unique_together = ("DeviceName", "SlaveId")
Device_Pydantic = pydantic_model_creator(Device, name="Device")
DeviceIn_Pydantic =pydantic_model_creator(Device, name="DeviceIn",exclude_readonly=True)

class PointConfig(Model):
    id = fields.IntField(pk=True)
    device = fields.ForeignKeyField(
        "models.Device",
        related_name="points",
        null=False,
    )
    PointName = fields.CharField(unique=False, max_length=20, null=False)
    class Meta:
        unique_together = ("device", "PointName")
    Len = fields.IntField(null=False)
    RegisterType = fields.CharField(unique=False, max_length=30, null=False)
    DataType = fields.CharField(unique=False, max_length=30, null=False)
    AccessType = fields.CharField(unique=False, max_length=30, null=False)
    Unit = fields.CharField(unique=False, max_length=30, null=False)
    ScaleFactor = fields.CharField(unique=False, max_length=30, null=False)
    Offset = fields.CharField(unique=False, max_length=30, null=False)
    Desc = fields.TextField(null=True)

PointConfig_Pydantic = pydantic_model_creator(PointConfig, name="PointConfig")
PointConfigIn_Pydantic = pydantic_model_creator(PointConfig, name="PointConfigIn", exclude_readonly=True)


class DataSave(Model):
    id = fields.IntField(pk=True)
    register_name = fields.TextField(unique=False, null=False)
    value = fields.FloatField(null=False)
    Device_Pk = fields.ForeignKeyField("models.Device", null=False, related_name="data_saves")
    PointConfig_Pk = fields.ForeignKeyField("models.PointConfig", null=False, related_name="point_saves")

DataSave_Pydantic = pydantic_model_creator(DataSave, name="DataSave")
DataSaveIn_Pydantic = pydantic_model_creator(DataSave, name="DataSaveIn", exclude_readonly=True)


class Prediction(Model):
    id = fields.IntField(pk=True)
    device = fields.ForeignKeyField("models.Device", related_name="device_predictions")
    point_config = fields.ForeignKeyField("models.PointConfig", related_name="point_predictions")
    timestamp = fields.DatetimeField(auto_now_add=True)
    predicted_value = fields.FloatField(null=False)
    model_name = fields.CharField(max_length=50, null=False)
    status = fields.CharField(max_length=20, default="completed")

Prediction_Pydantic = pydantic_model_creator(Prediction, name="Prediction")
PredictionIn_Pydantic = pydantic_model_creator(Prediction, name="PredictionIn", exclude_readonly=True)