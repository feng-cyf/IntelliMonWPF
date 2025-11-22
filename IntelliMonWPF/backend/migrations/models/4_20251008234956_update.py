from tortoise import BaseDBAsyncClient


async def upgrade(db: BaseDBAsyncClient) -> str:
    return """
        ALTER TABLE `user` MODIFY COLUMN `pwd_hash` VARCHAR(150) NOT NULL;
        ALTER TABLE `user` ADD UNIQUE INDEX `uid_user_usernam_96d166` (`username`, `pwd_hash`);"""


async def downgrade(db: BaseDBAsyncClient) -> str:
    return """
        ALTER TABLE `user` DROP INDEX `uid_user_usernam_96d166`;
        ALTER TABLE `user` MODIFY COLUMN `pwd_hash` LONGTEXT NOT NULL;"""
