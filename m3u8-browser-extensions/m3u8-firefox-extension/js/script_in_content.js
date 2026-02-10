const root = browser; //chrome;
// set handler to tabs:  need for seng objects to backgroung.js
if (root.extension.onConnect) {
    root.extension.onConnect.addListener(port => port.onMessage.addListener(popupInfo_methodCaller));
}

/* Function will be called from background.js */
function popupInfo_init() { window.popupInfo = new popupInfoType(); }

/* Function will be called when background.js send some data by port interface */
function popupInfo_methodCaller(obj) {
    if (obj?.method) {
        window.popupInfo[obj.method](obj?.data);
    }
}

window.popupInfoType = function () { };
window.popupInfoType.prototype = {
    tab_id   : null,
    port     : null,
    m3u8_urls: [],

    /* Function will be called from backgroung.js */
    setTabId: function (tab_id) { this.tab_id = tab_id; },

    setM3u8Urls: function (m3u8_urls) { this.m3u8_urls = m3u8_urls; },
    getM3u8Urls: function () { return (this.m3u8_urls); },

    /* Function check m3u8 urls count */
    connect2Extension: function () {
        if (root.extension.connect) {
            // create connection to backgroung.html and send request
            this.port = root.extension.connect();
            // send count of setUrlsCountText
            this.port.postMessage({ method: 'setUrlsCountText', data: { tabId: this.tab_id } });
        }
    }
};
