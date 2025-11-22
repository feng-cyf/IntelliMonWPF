import logging
import sys

def setup_logging():
    # 1. 先给 Windows 终端启用 ANSI 颜色（避免 Windows cmd 不显示颜色）
    if sys.platform.startswith("win"):
        import ctypes
        kernel32 = ctypes.windll.kernel32
        kernel32.SetConsoleMode(kernel32.GetStdHandle(-11), 7)  # 启用 ANSI 转义序列

    # 2. 定义「日志级别 + 消息内容」的颜色（INFO 级别全白）
    LOG_COLORS = {
        logging.DEBUG: "\033[34m",  # 蓝色：DEBUG（级别名 + 消息）
        logging.INFO: "\033[37m",  # 白色：INFO（级别名 + 消息，你要的效果）
        logging.WARNING: "\033[33m",  # 黄色：WARNING（级别名 + 消息）
        logging.ERROR: "\033[31m",  # 红色：ERROR（级别名 + 消息）
        logging.CRITICAL: "\033[41m"  # 红色背景：CRITICAL（级别名 + 消息）
    }

    # 3. 自定义格式化器：让「级别名 + 消息内容」都带颜色
    class ColorFormatter(logging.Formatter):
        def format(self, record):
            # 获取当前日志级别的颜色码
            level_color = LOG_COLORS.get(record.levelno, "\033[0m")  # 默认无颜色
            reset_color = "\033[0m"  # 重置颜色（避免后续日志继承颜色）

            # 1. 给「日志级别名」加颜色（如 [INFO]）
            record.levelname = f"{level_color}{record.levelname}{reset_color}"

            # 2. 给「日志消息内容」加颜色（这是关键！让消息跟着级别变色）
            record.msg = f"{level_color}{record.msg}{reset_color}"

            # 3. 生成最终日志（时间、模块名是默认颜色，级别和消息是自定义颜色）
            log_format = "%(asctime)s - %(name)s - %(levelname)s - %(message)s"
            return logging.Formatter(log_format).format(record)

    # 4. 配置根日志器
    root_logger = logging.getLogger()
    root_logger.setLevel(logging.DEBUG)  # 显示所有级别日志
    root_logger.handlers.clear()  # 清空默认 handler，避免重复输出

    # 5. 添加控制台 handler（绑定彩色格式化器）
    console_handler = logging.StreamHandler()
    console_handler.setFormatter(ColorFormatter())
    root_logger.addHandler(console_handler)

    return root_logger