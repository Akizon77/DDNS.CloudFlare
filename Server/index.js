const http = require('http');
const fs = require('fs');
const url = require('url');
const axios = require('axios');

//New Server
const server = http.createServer((req, res) => {

    const query = url.parse(req.url, true).query;
    const cdata = query.cdata;
    // 在此处校验参数！
    if (cdata === 'c') {
        //unlink 删除文件
        fs.unlink('cdata.txt', (err) => {
            if (err) {
                console.error('Error deleting file:', err);
            }
            else {
                console.log('Deleted');
            }
        });
        const currentData = new Date().toLocaleString();
        //记录请求
        fs.appendFile('cdata.txt', currentData, (err) => {
            if (err) {
                res.statusCode = 500;
                res.end('Internal Server Error');
            }
            else {
                res.statusCode = 200;
                res.end(currentData);
            }
        });
    }
    else {
        res.statusCode = 400;
        res.end('Bad Request');
    }
});
// 检查15分钟内是否有请求
setInterval(() => {
    const currentTime = new Date().getTime();
    fs.stat('cdata.txt', (err, stats) => {
        if (err) {
            console.error('Error checking request log:', err);
        } else {
            const lastModifiedTime = stats.mtime.getTime();
            const timeDiff = currentTime - lastModifiedTime;
            if (timeDiff > 15 * 60 * 1000) {
                axios.get('https://api.telegram.org/botXXXX:YYYY/sendMessage?chat_id=XXXX&text=%E6%9C%8D%E5%8A%A1%E5%99%A8%E5%B7%B2%E7%A6%BB%E7%BA%BF%27')
                    .then((response) => {
                        console.log('Push Telegram message successfully');
                    })
                    .catch((error) => {
                        console.error('Error push Telegram Message:', error);
                    });
            }
        }
    });
}, 15 * 60 * 1000);

//Listen the port 231000
server.listen(23100, () => {
    console.log('Server is running on pory 23100');
});