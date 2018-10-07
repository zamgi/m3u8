// set handler to tabs:  need for seng objects to backgroung.js
chrome.extension.onConnect.addListener(function (port) {
    port.onMessage.addListener(methodCaller);
});

/* Functino will be called from background.js */
function initialization() {
    window.popup = new popupObj();
}

/* Functino will be called when background.js send some data by port interface */
function methodCaller(obj) {
    if (obj && obj.method) {
        if (obj.data)
            window.popup[obj.method](obj.data);
        else
            window.popup[obj.method]();
    }
}

/* Popup object */
window.popupObj = function () { };

/* Public methods */
window.popupObj.prototype = {
    /* internal params */
    tab_id: null,
    port: null,
    m3u8_urls: [],

    /* Function will be called from backgroung.js */
    setTabId: function (id) {
        this.tab_id = id;
    },

    set_m3u8_urls: function (m3u8_urls) {
        this.m3u8_urls = m3u8_urls;
    },
    get_m3u8_urls: function () {
        return (this.m3u8_urls);
    },

    /* Function check m3u8 urls count */
    run: function () {
        // create connection to backgroung.html and send request
        this.port = chrome.extension.connect();
        // send count of m3u8_urls_count
        this.port.postMessage({ method: 'm3u8_urls_count', data: { tab_id: this.tab_id } });
    }
};
