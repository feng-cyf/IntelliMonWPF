from tortoise import BaseDBAsyncClient


async def upgrade(db: BaseDBAsyncClient) -> str:
    return """
        ALTER TABLE `pointconfig` ADD UNIQUE INDEX `uid_pointconfig_device__df4c01` (`device_id`, `PointName`);
        ALTER TABLE `pointconfig` ADD UNIQUE INDEX `PointName` (`PointName`);"""


async def downgrade(db: BaseDBAsyncClient) -> str:
    return """
        ALTER TABLE `pointconfig` DROP INDEX `PointName`;
        ALTER TABLE `pointconfig` DROP INDEX `uid_pointconfig_device__df4c01`;"""
