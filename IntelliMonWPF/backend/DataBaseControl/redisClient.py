import asyncio
import redis.asyncio as redis

class RedisClient:
    _instance = None
    _lock = asyncio.Lock()
    _initialized = False

    def __new__(cls, *args, **kwargs):
        if cls._instance is None:
            cls._instance = super().__new__(cls)
        return cls._instance

    def __init__(self):
        if not self._initialized:
            self._pool = None
            self._client = None
            self._initialized = True

    async def init(self, host="localhost", port=6379, db=0, max_connections=20):
        async with self._lock:
            if self._pool is None:
                self._pool = redis.ConnectionPool(
                    host=host,
                    port=port,
                    db=db,
                    decode_responses=True,
                    max_connections=max_connections
                )
                self._client =redis.Redis(connection_pool=self._pool)

    async def get_redis(self):
        if self._client is None:
            raise RuntimeError("RedisClient not initialized. Call init() first.")
        return self._client

    async def close(self):
        if self._pool:
            await self._pool.disconnect()
            self._pool = None
            self._client = None