window.onload = function () {
    // get m3u8 urls for current active tab
    window.bg_wnd = chrome.extension.getBackgroundPage();
    var m3u8_urls = window.bg_wnd.bg.get_m3u8_urls();

    // function render m3u8 urls list
    render_m3u8_urls(m3u8_urls);
};

function render_m3u8_urls(m3u8_urls) {
    var content = document.getElementById('content');

    if (!m3u8_urls || !m3u8_urls.length) {
        content.innerHTML = '<h5 class="not-found">m3u8 ulrs not found on this page</h5>';
        return;
    }
    
    var trs = [];
    for (var i = 0, cnt = m3u8_urls.length; i < cnt; i++) {
        var m3u8_url = m3u8_urls[i];
        trs.push('<tr><td class="content" title="' + m3u8_url + '"><a href="' + m3u8_url + '">' + m3u8_url + '</a></td></tr>' );
    }
    content.innerHTML = '<h5 class="found">m3u8 ulrs: ' + m3u8_urls.length + '</h5>' +
                        '<table class="content">' + trs.join('') + '</table>';

    var aa = content.querySelectorAll('a');
    for (var i = 0; i < aa.length; i++) {
        aa[i].addEventListener('click', function (event) {
            connect2host(this.href);
            event.preventDefault();
            return (false);
        });
    }
};

function connect2host(m3u8_url, auto_start_download) {
    var hostName = "m3u8.downloader.host";

    chrome.runtime.sendNativeMessage(hostName,
        {
            m3u8_url: m3u8_url
            /*, auto_start_download: !!auto_start_download*/
        },
        function (response) {
            console.log("received: " + response);
        });
};
