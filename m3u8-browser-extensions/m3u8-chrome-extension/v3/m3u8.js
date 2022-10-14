window.onload = async function () {
    let tabs  = await chrome.tabs.query({ active: true, currentWindow: true });
    let tabId = ((tabs && tabs.length) ? tabs[ 0 ].id : -1);

    let res = await chrome.storage.local.get();
    let workInfo = new workInfoType(res.workInfo);

    var m3u8_urls = workInfo.get_m3u8_urls(tabId);

    // function render m3u8 urls list
    render_m3u8_urls(m3u8_urls);

    let ch = document.getElementById('saveUrlListBetweenTabReload');
    ch.checked = !!res.saveUrlListBetweenTabReload;    
    ch.addEventListener('click', async function (event) {
        await chrome.storage.local.set({ saveUrlListBetweenTabReload: this.checked });
    });

    let content = document.getElementById('content');
    ch = document.getElementById('directionRtl');
    ch.checked = (res.directionRtl !== undefined) ? !!res.directionRtl : (content.style.direction === 'rtl');
    content.style.direction = ch.checked ? 'rtl' : '';
    ch.addEventListener('click', async function (event) {
        content.style.direction = this.checked ? 'rtl' : '';
        await chrome.storage.local.set({ directionRtl: this.checked });
    });

    if (m3u8_urls && m3u8_urls.length) {
        let bt = document.getElementById('clearUrlList');
        bt.style.display = '';
        bt.addEventListener('click', async function (event) {
            await workInfo.deleteTabUrls(tabId);

            render_m3u8_urls();
            this.style.display = 'none';
        });
    }    
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
            send2host_single(this.href);
            event.preventDefault();
            return (false);
        });
    }

    aa = content.querySelectorAll('a.auto_start_download');
    for (i = 0; i < aa.length; i++) {
        aa[i].addEventListener('click', function (event) {
            send2host_single(this.href, true);
            event.preventDefault();
            return (false);
        });
    }

    aa = content.querySelectorAll('a.download_all');
    if (0 < aa.length) {
        aa[0].addEventListener('click', function (event) {
            var messageObject = [], bb = content.querySelectorAll('a.download');
            for (var j = 0; j < bb.length; j++) {
                messageObject.push( create_messageObject(bb[j].href) );
            }
            send2host_multi(messageObject);
            event.preventDefault();
            return (false);
        });
    }

    aa = content.querySelectorAll('a.auto_start_download_all');
    if (0 < aa.length) {
        aa[0].addEventListener('click', function (event) {
            var messageObject = [], bb = content.querySelectorAll('a.auto_start_download');
            for (var j = 0; j < bb.length; j++) {
                messageObject.push( create_messageObject(bb[j].href, true) );
            }
            send2host_multi(messageObject);
            event.preventDefault();
            return (false);
        });
    }
}

function create_messageObject(m3u8_url, auto_start_download) {
    return ({
        m3u8_url: m3u8_url,
        auto_start_download: !!auto_start_download
    });
}
async function send2host_single(m3u8_url, auto_start_download) { await send2host_multi( [ create_messageObject(m3u8_url, auto_start_download) ] ); }
async function send2host_multi(messageObject) {
    var HOST_NAME = "m3u8.downloader.host";

    let res = await chrome.runtime.sendNativeMessage(HOST_NAME, { array: messageObject });
    var message;
    if (res) {
        if (res.text === "success") {
            console.log("[" + HOST_NAME + "] sent the response: '" + JSON.stringify(res) + "'");
            return;
        }

        message = res.text || JSON.stringify(res);
    }
    else if (chrome.runtime.lastError && chrome.runtime.lastError.message) {
        message = chrome.runtime.lastError.message;
    }

    var notificationOptions = {
        type    : "basic",
        title   : "[" + HOST_NAME + "] => send-native-message ERROR:",
        message : message || "[NULL]",
        iconUrl : "m3u8_148.png",
        priority: 2
    };
    await chrome.notifications.clear(HOST_NAME);
    await chrome.notifications.create(HOST_NAME, notificationOptions);
}
