window.onload = function () {
    try {
        // get m3u8 urls for current active tab
        let backgroundPage = chrome.extension.getBackgroundPage();
        let t = backgroundPage.workInfo.getM3u8Urls();
        let m3u8_urls = t.m3u8_urls;

        render_m3u8_urls(m3u8_urls, t.requestHeaders);

        //let ch = document.getElementById('saveUrlListBetweenTabReload');
        //ch.checked = !!res.saveUrlListBetweenTabReload;
        //ch.addEventListener('click', async function (/*e*/) {
        //    await chrome.storage.local.set({ saveUrlListBetweenTabReload: this.checked });
        //});

        let content = document.getElementById('content');
        let ch = document.getElementById('directionRtl');
        ch.checked = /*(res.directionRtl !== undefined) ? !!res.directionRtl :*/ (content.style.direction === 'rtl');
        content.style.direction = ch.checked ? 'rtl' : '';
        ch.addEventListener('click', async function (/*e*/) {
            content.style.direction = this.checked ? 'rtl' : '';
            //await chrome.storage.local.set({ directionRtl: this.checked });
        });

        if (m3u8_urls && m3u8_urls.length) {
            let bt = document.getElementById('clearUrlList');
            bt.style.display = '';
            bt.addEventListener('click', async function (/*e*/) {
                render_m3u8_urls();
                this.style.display = 'none';

                await backgroundPage.workInfo.deleteTabUrls(backgroundPage.workInfo.active_tabId/*tabId*/);
            });
        }

    } catch (ex) {
        console.error("m3u8 => " + ex);
        render_m3u8_urls();
    }
};

function render_m3u8_urls(m3u8_urls, requestHeaders) {
    let content = document.getElementById('content');

    if (!m3u8_urls || !m3u8_urls.length) {
        content.innerHTML = '<h5 class="not-found">m3u8 requests no were made from this page</h5>';
        return;
    }
    
    let trs = [];
    for (let i = 0, cnt = m3u8_urls.length; i < cnt; i++) {
        let m3u8_url = m3u8_urls[i], requestHeaders_4_url = requestHeaders[m3u8_url] || '';
        let a = document.createElement('a'); a.className = 'auto_start_download'; a.title = 'auto start download'; a.href = m3u8_url; a.setAttribute('requestHeaders', requestHeaders_4_url);

        let img = document.createElement('img'); img.src = 'img/auto_start_download.png'; img.style.height = '16px';
        a.appendChild(img);

        let td = document.createElement('td');
        td.appendChild(a);

        let a2 = document.createElement('a'); a2.className = 'download'; a2.href = m3u8_url; a2.setAttribute('requestHeaders', requestHeaders_4_url);
        a2.appendChild(document.createTextNode(m3u8_url));

        let td2 = document.createElement('td'); td2.className = 'content'; td2.title = m3u8_url;
        td2.appendChild(a2);

        let tr = document.createElement('tr');
        tr.appendChild(td);
        tr.appendChild(td2);

        trs.push(tr.outerHTML);

        //trs.push('<tr><td><a class="auto_start_download" title="auto start download" href="' + m3u8_url + '"><img src="img/auto_start_download.png" style="height: 16px"/></a></td>' +
        //         '<td class="content" title="' + m3u8_url + '"><a class="download" href="' + m3u8_url + '">' + m3u8_url + '</a></td></tr>');
    }
    let download_all = '<h5 class="found"><a class="download_all" title="download all" href="#">m3u8 urls: ' + m3u8_urls.length + '</a></h5>';
    let auto_start_download_all = ((1 < m3u8_urls.length) ? '<a class="auto_start_download_all" title="auto start download all" href="#"><img src="img/auto_start_download.png" style="height: 16px"/></a>' : '');
    content.innerHTML = '<table><tr><td>' + auto_start_download_all + '</td><td>' + download_all + '</td></tr></table>' +
                        '<table class="content">' + trs.join('') + '</table>';

    let aa = content.querySelectorAll('a.download');
    for (let i = 0; i < aa.length; i++) {
        aa[i].addEventListener('click', function (e) {                     
            send2host_single(this.href, this.getAttribute('requestHeaders') || '');
            e.preventDefault();
            return (false);
        });
    }

    aa = content.querySelectorAll('a.auto_start_download');
    for (let i = 0; i < aa.length; i++) {
        aa[i].addEventListener('click', function (e) {            
            send2host_single(this.href, this.getAttribute('requestHeaders') || '', true);
            e.preventDefault();
            return (false);
        });
    }

    aa = content.querySelectorAll('a.download_all');
    if (0 < aa.length) {
        aa[0].addEventListener('click', function (e) {
            let messageObject = [], bb = content.querySelectorAll('a.download');
            for (let j = 0; j < bb.length; j++) {
                messageObject.push( create_messageObject(bb[j].href, bb[j].getAttribute('requestHeaders') || '') );
            }
            send2host_multi(messageObject);
            e.preventDefault();
            return (false);
        });
    }

    aa = content.querySelectorAll('a.auto_start_download_all');
    if (0 < aa.length) {
        aa[0].addEventListener('click', function (e) {
            let messageObject = [], bb = content.querySelectorAll('a.auto_start_download');
            for (let j = 0; j < bb.length; j++) {
                messageObject.push( create_messageObject(bb[j].href, bb[j].getAttribute('requestHeaders') || '', true) );
            }
            send2host_multi(messageObject);
            e.preventDefault();
            return (false);
        });
    }
}

function create_messageObject(m3u8_url, requestHeaders, auto_start_download) {
    return ({
        m3u8_url: m3u8_url,
        requestHeaders: requestHeaders,
        auto_start_download: !!auto_start_download
    });
}
function send2host_single(m3u8_url, requestHeaders, auto_start_download) { send2host_multi( [ create_messageObject(m3u8_url, requestHeaders, auto_start_download) ] ); }
function send2host_multi(messageObject) {
    const HOST_NAME = "m3u8.downloader.host";

    try {
        chrome.runtime.sendNativeMessage(HOST_NAME, { array: messageObject },
        function (response) {
            let message;
            if (response) {
                if (response.text === "success") {
                    console.log("[" + HOST_NAME + "] sent the response: '" + JSON.stringify(response) + "'");
                    return;
                }

                message = response.text || JSON.stringify(response);
            }
            else if (chrome.runtime.lastError && chrome.runtime.lastError.message) {
                message = chrome.runtime.lastError.message;
            }

            let notificationOptions = {
                type    : "basic",
                title   : "[" + HOST_NAME + "] => send-native-message ERROR:",
                message : message || "[NULL]",
                iconUrl : "img/m3u8_148.png",
                priority: 2
            };
            chrome.notifications.clear(HOST_NAME);
            chrome.notifications.create(HOST_NAME, notificationOptions);
        });        
    } catch (ex) {
        let message = ex + '';
        let notificationOptions = {
            type    : "basic",
            title   : "[" + HOST_NAME + "] => send-native-message ERROR:",
            message : message || "[NULL]",
            iconUrl : "img/m3u8_148.png",
            priority: 2
        };
        chrome.notifications.clear(HOST_NAME);
        chrome.notifications.create(HOST_NAME, notificationOptions);
    }
}
