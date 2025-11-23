import os

from dotenv import load_dotenv

load_dotenv("./database.env")
TORTOISE_ORM = {
    "connections": {
        "default": {
            "engine": "tortoise.backends.mysql",
            "credentials": {
                "host": "127.0.0.1",
                "port": 3306,
                "user": "root",
                "password": os.getenv("mysql_pwd"),
                "database": os.getenv("db"),
            }
        }
    },
    "apps": {
        "models": {
            "models": ["orm.model", "aerich.models"],
            "default_connection": "default",
        }
    }
}