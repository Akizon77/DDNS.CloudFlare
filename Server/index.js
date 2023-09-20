const http = require("http");
const fs = require("fs");
const url = require("url");
const axios = require("axios");

//运行先创建文件，避免检测不到
fs.appendFile("cdata.txt", "Create", (err) => {});

//New Server
const server = http.createServer((req, res) => {
  const query = url.parse(req.url, true).query;
  const cdata = query.cdata;
  // 在此处校验参数！
  if (cdata === "c") {
    //unlink 删除文件
    fs.unlink("cdata.txt", (err) => {
      if (err) {
        console.error("Error deleting file:", err, new Date().toLocaleString());
      } else {
        console.log("Deleted", new Date().toLocaleString());
      }
    });
    const currentData = new Date().toLocaleString();
    //记录请求
    fs.appendFile("cdata.txt", currentData, (err) => {
      if (err) {
        res.statusCode = 500;
        res.end("Internal Server Error");
      } else {
        fs.readFile(
          "cdata.txt",
          "utf8",
          (err, data) => {
            if (err) {
              res.statusCode = 500;
              res.end("Internal Server Error");
            } else {
              res.statusCode = 200;
              res.end(data);
            }
          }
        );
      }
    });
  } else {
    res.statusCode = 400;
    res.end("Bad Request");
  }
});
// 检查15分钟内是否有请求
setInterval(() => {
  const currentTime = new Date().getTime();
  fs.stat("cdata.txt", (err, stats) => {
    if (err) {
      console.error(
        "Error checking request log:",
        err,
        new Date().toLocaleString()
      );
    } else {
      const lastModifiedTime = stats.mtime.getTime();
      const timeDiff = currentTime - lastModifiedTime;
      if (timeDiff > 12 * 60 * 1000) {
        PushMessage();
      }
    }
  });
}, 1 * 60 * 1000);
//推送消息，直到成功
function PushMessage() {
  axios
    .get("https://api.telegram.org/")
    .then((response) => {
      console.log(
        "Push Telegram message successfully",
        new Date().toLocaleString()
      );
    })
    .catch((error) => {
      console.error(
        "Error push Telegram Message:",
        error,
        new Date().toLocaleString()
      );
      PushMessage();
    });
}

//Listen the port 231000
server.listen(23100, () => {
  console.log("Server is running on pory 23100", new Date().toLocaleString());
});
