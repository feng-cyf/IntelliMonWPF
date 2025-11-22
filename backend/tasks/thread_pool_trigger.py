import asyncio
import logging
import threading
from concurrent.futures import ThreadPoolExecutor
from enum import Enum, auto
from functools import partial

global_thread_pool = ThreadPoolExecutor(max_workers=10)

class TaskStatus(Enum):
    INIT = auto()  # 初始化状态
    SUBMITTED = auto()  # 已提交到线程池
    RUNNING = auto()  # 正在执行
    COMPLETED = auto()  # 已完成
    FAILED = auto()  # 执行失败


class ImportThread:
    def __init__(self, task, *task_args, **task_kwargs):
        self.task = task
        self.task_args = task_args
        self.task_kwargs = task_kwargs
        self.trigger_event = threading.Event()
        self.future = None
        self.status = TaskStatus.INIT
        self.result = None

    def _run_work(self):
        self.trigger_event.wait()

        self.status = TaskStatus.RUNNING
        try:
            if asyncio.iscoroutinefunction(self.task):
                loop = asyncio.new_event_loop()
                asyncio.set_event_loop(loop)
                try:
                    self.result = loop.run_until_complete(
                        self.task(*self.task_args, **self.task_kwargs
                                  ))
                finally:
                    loop.close()
            else:
                self.result = self.task(*self.task_args, **self.task_kwargs)

            self.status = TaskStatus.COMPLETED
            return self.result
        except Exception as e:
            logging.exception("任务执行出错")
            self.status = TaskStatus.FAILED
            raise

    def submit(self):
        if self.status != TaskStatus.INIT:
            raise RuntimeError("任务已提交或正在执行")

        self.future = global_thread_pool.submit(self._run_work)
        self.status = TaskStatus.SUBMITTED
        return self.future

    def trigger(self):
        if self.status != TaskStatus.SUBMITTED:
            raise RuntimeError("任务尚未提交或已执行")

        self.trigger_event.set()

    def get_result(self, timeout=None):
        if self.future is None:
            raise RuntimeError("任务尚未提交")

        return self.future.result(timeout)

    def cancel(self):
        if self.future and self.status == TaskStatus.SUBMITTED:
            return self.future.cancel()
        return False