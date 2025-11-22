from tortoise import BaseDBAsyncClient


async def upgrade(db: BaseDBAsyncClient) -> str:
    return """
        ALTER TABLE `pointconfig` DROP INDEX `PointName`;"""


async def downgrade(db: BaseDBAsyncClient) -> str:
    return """
        ALTER TABLE `pointconfig` ADD UNIQUE INDEX `PointName` (`PointName`);"""
