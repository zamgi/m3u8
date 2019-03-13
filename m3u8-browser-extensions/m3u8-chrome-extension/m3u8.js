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
        content.innerHTML = '<h5 class="not-found">m3u8 requests no were made from this page</h5>';
        return;
    }
    
    var trs = [];
    for (var i = 0, cnt = m3u8_urls.length; i < cnt; i++) {
        var m3u8_url = m3u8_urls[i];
        trs.push('<tr><td><a class="auto_start_download" title="auto start download" href="' + m3u8_url + '"><img src="auto_start_download.png" style="height: 16px"/></a></td>' + 
            '<td class="content" title="' + m3u8_url + '"><a class="download" href="' + m3u8_url + '">' + m3u8_url + '</a></td></tr>');
    }
    var download_all = '<h5 class="found"><a class="download_all" title="download all" href="#">m3u8 urls: ' + m3u8_urls.length + '</a></h5>';
    var auto_start_download_all = ((1 < m3u8_urls.length) ? '<a class="auto_start_download_all" title="auto start download all" href="#"><img src="auto_start_download.png" style="height: 16px"/></a>' : '');
    content.innerHTML = '<table><tr><td>' + auto_start_download_all + '</td><td>' + download_all + '</td></tr></table>' +
                        '<table class="content">' + trs.join('') + '</table>';

    var aa = content.querySelectorAll('a.download');
    for (i = 0; i < aa.length; i++) {
        aa[i].addEventListener('click', function (event) {
            connect2host(this.href);
            event.preventDefault();
            return (false);
        });
    }

    aa = content.querySelectorAll('a.auto_start_download');
    for (i = 0; i < aa.length; i++) {
        aa[i].addEventListener('click', function (event) {
            connect2host(this.href, true);
            event.preventDefault();
            return (false);
        });
    }

    aa = content.querySelectorAll('a.download_all');
    if (0 < aa.length) {
        aa[0].addEventListener('click', function (event) {
            var bb = content.querySelectorAll('a.download');
            for (var j = 0; j < bb.length; j++) {
                connect2host(bb[j].href, false);
            }
            event.preventDefault();
            return (false);
        });
    }

    aa = content.querySelectorAll('a.auto_start_download_all');
    if (0 < aa.length) {
        aa[0].addEventListener('click', function (event) {
            var bb = content.querySelectorAll('a.auto_start_download');
            for (var j = 0; j < bb.length; j++) {
                connect2host(bb[j].href, true);
            }
            event.preventDefault();
            return (false);
        });
    }
}

function connect2host(m3u8_url, auto_start_download) {
    var hostName = "m3u8.downloader.host";

    chrome.runtime.sendNativeMessage(hostName,
        {
            m3u8_url: m3u8_url,
            auto_start_download: !!auto_start_download
        },
        function (response) {
            if (response) {
                console.log("[" + hostName + "] sent the response: '" + JSON.stringify(response) + "'");
            }
            else if (chrome.runtime.lastError && chrome.runtime.lastError.message) {
                var content = document.getElementById('content');
                content.innerHTML = "<div style='color: red'><b>" + chrome.runtime.lastError.message + "</b></div>" + content.innerHTML;
            }
        });
}
