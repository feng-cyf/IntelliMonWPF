from tortoise import BaseDBAsyncClient


async def upgrade(db: BaseDBAsyncClient) -> str:
    return """
        CREATE TABLE IF NOT EXISTS `datasave` (
    `id` INT NOT NULL PRIMARY KEY AUTO_INCREMENT,
    `register_name` LONGTEXT NOT NULL,
    `value` DOUBLE NOT NULL,
    `Device_Pk_id` INT NOT NULL,
    `PointConfig_Pk_id` INT NOT NULL,
    CONSTRAINT `fk_datasave_device_3a18b2ef` FOREIGN KEY (`Device_Pk_id`) REFERENCES `device` (`id`) ON DELETE CASCADE,
    CONSTRAINT `fk_datasave_pointcon_4cb4061d` FOREIGN KEY (`PointConfig_Pk_id`) REFERENCES `pointconfig` (`id`) ON DELETE CASCADE
) CHARACTER SET utf8mb4;
        CREATE TABLE IF NOT EXISTS `device` (
    `id` INT NOT NULL PRIMARY KEY AUTO_INCREMENT,
    `DeviceName` VARCHAR(60) NOT NULL,
    `SlaveId` INT NOT NULL,
    UNIQUE KEY `uid_device_DeviceN_6754df` (`DeviceName`, `SlaveId`)
) CHARACTER SET utf8mb4;
        CREATE TABLE IF NOT EXISTS `pointconfig` (
    `id` INT NOT NULL PRIMARY KEY AUTO_INCREMENT,
    `PointName` VARCHAR(20) NOT NULL,
    `Len` INT NOT NULL,
    `RegisterType` VARCHAR(30) NOT NULL,
    `DataType` VARCHAR(30) NOT NULL,
    `AccessType` VARCHAR(30) NOT NULL,
    `Unit` VARCHAR(30) NOT NULL,
    `ScaleFactor` VARCHAR(30) NOT NULL,
    `Offset` VARCHAR(30) NOT NULL,
    `Desc` LONGTEXT,
    `device_id` INT NOT NULL,
    CONSTRAINT `fk_pointcon_device_e35ffb12` FOREIGN KEY (`device_id`) REFERENCES `device` (`id`) ON DELETE CASCADE
) CHARACTER SET utf8mb4;
        CREATE TABLE IF NOT EXISTS `prediction` (
    `id` INT NOT NULL PRIMARY KEY AUTO_INCREMENT,
    `timestamp` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `predicted_value` DOUBLE NOT NULL,
    `model_name` VARCHAR(50) NOT NULL,
    `status` VARCHAR(20) NOT NULL DEFAULT 'completed',
    `device_id` INT NOT NULL,
    `point_config_id` INT NOT NULL,
    CONSTRAINT `fk_predicti_device_d3b4789f` FOREIGN KEY (`device_id`) REFERENCES `device` (`id`) ON DELETE CASCADE,
    CONSTRAINT `fk_predicti_pointcon_67d0f175` FOREIGN KEY (`point_config_id`) REFERENCES `pointconfig` (`id`) ON DELETE CASCADE
) CHARACTER SET utf8mb4;"""


async def downgrade(db: BaseDBAsyncClient) -> str:
    return """
        DROP TABLE IF EXISTS `datasave`;
        DROP TABLE IF EXISTS `device`;
        DROP TABLE IF EXISTS `pointconfig`;
        DROP TABLE IF EXISTS `prediction`;"""
