TORTOISE_ORM = {
    "connections": {
        "default": {
            "engine": "tortoise.backends.mysql",
            "credentials": {
                "host": "127.0.0.1",
                "port": 3306,
                "user": "root",
                "password": "Jwg051113.",
                "database": "data_save",
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