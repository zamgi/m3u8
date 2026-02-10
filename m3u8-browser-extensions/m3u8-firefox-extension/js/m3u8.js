const root = browser; //chrome;
window.addEventListener('load', async function () {
    try {
        // get m3u8 urls for current active tab
        let backgroundPage = root.extension.getBackgroundPage();
        let t = backgroundPage.workInfo.getM3u8Urls();
        let m3u8_urls = t.m3u8_urls;

        let opt = await root.storage.local.get(); if (!opt) opt = {}; if (opt.groupByAudioVideo === undefined) opt.groupByAudioVideo = true;
                                                                      if (opt.directionRtl      === undefined) opt.directionRtl      = true;
        const renderFunc = async function (groupByAudioVideo) {
            if (groupByAudioVideo) {
                let urls = group_by_audio_video(m3u8_urls, t.requestHeaders);
                render_grouped_m3u8_urls(urls);
            } else {
                render_m3u8_urls(m3u8_urls, t.requestHeaders);
            }
        }
        let ch = document.getElementById('groupByAudioVideo');
        ch.addEventListener('click', async function () {
            renderFunc(this.checked);
            opt.groupByAudioVideo = this.checked;
            await root.storage.local.set(opt);
        });
        ch.checked = !!opt.groupByAudioVideo;
        renderFunc(ch.checked);

        let content = document.getElementById('content');
        ch = document.getElementById('directionRtl');
        ch.checked = !!opt.directionRtl; //(content.style.direction === 'rtl');
        content.style.direction = ch.checked ? 'rtl' : '';
        ch.addEventListener('click', async function () {
            content.style.direction = this.checked ? 'rtl' : '';
            opt.directionRtl = this.checked;
            await root.storage.local.set(opt);
        });

        //ch = document.getElementById('saveUrlListBetweenTabReload');
        //ch.checked = !!opt.saveUrlListBetweenTabReload;
        //ch.addEventListener('click', async function () { await root.storage.local.set({ saveUrlListBetweenTabReload: this.checked }); });

        if (m3u8_urls?.length) {
            let bt = document.getElementById('clearUrlList');
            bt.style.display = '';
            bt.addEventListener('click', async function (/*e*/) {
                render_m3u8_urls();
                this.style.display = 'none';

                await backgroundPage.workInfo.deleteTabUrls(backgroundPage.workInfo.active_tabId/*tabId*/);
            });
        }

    } catch (ex) {
        console.error('m3u8 => ' + ex);
        render_m3u8_urls();
    }
});

function get_trimmed_url(url, max_len) {
    if (!max_len) max_len = 120;
    if (max_len < url?.length) {
        url = url.substring(0, max_len / 2 - 2) + '...' + url.substring(url.length - max_len/2 - 2);
    }
    return (url);
}
function group_by_audio_video(m3u8_urls, requestHeaders) {
    if (!m3u8_urls?.length) return;

    let res = [], skiped_urls = {};
    for (let i = 0, cnt = m3u8_urls.length; i < cnt; i++) {
        let m3u8_url = m3u8_urls[i];
        if (skiped_urls[m3u8_url]) continue;
        let requestHeaders_4_url = requestHeaders[m3u8_url] || '';

        const SUFFIX_v = '-v1.m3u8', SUFFIX_a = '-a1.m3u8';
        let is_video = m3u8_url.endsWith(SUFFIX_v),
            is_audio = !is_video && m3u8_url.endsWith(SUFFIX_a);
        if (is_video || is_audio) {
            let suf_v = is_video ? SUFFIX_v : SUFFIX_a,
                suf_a = is_video ? SUFFIX_a : SUFFIX_v;

            let grouped_url = m3u8_url.substring(0, m3u8_url.length - suf_v.length),
                url_av = grouped_url + suf_a,
                idx   = m3u8_urls.indexOf(url_av);
            if (idx !== -1) {
                skiped_urls[url_av] = 1;

                let v = { url: m3u8_url, requestHeaders: requestHeaders_4_url },
                    a = { url: url_av  , requestHeaders: requestHeaders[url_av] || '' };
                let o = {
                    is_group_by_audio_video: true,
                    grouped_url_text: grouped_url + ' + (' + suf_a + ')+(' + suf_v + ')',
                    video: is_video ? v : a,
                    audio: is_video ? a : v
                };
                res.push(o);
                continue;
            }
        }

        //if (m3u8_url.endsWith(SUFFIX_v)) {
        //    let grouped_url = m3u8_url.substring(0, m3u8_url.length - SUFFIX_v.length),
        //        url_a = grouped_url + SUFFIX_a,
        //        idx   = m3u8_urls.indexOf(url_a);
        //    if (idx !== -1) {
        //        skiped_urls[url_a] = 1;

        //        let o = {
        //            is_group_by_audio_video: true,
        //            grouped_url_text: grouped_url + ' + (' + SUFFIX_a + ') + (' + SUFFIX_v + ')',
        //            video: { url: m3u8_url, requestHeaders: requestHeaders_4_url },
        //            audio: { url: url_a   , requestHeaders: requestHeaders[url_a] || ''}
        //        };
        //        res.push(o);
        //        continue;
        //    }
        //}

        res.push({ url: m3u8_url, requestHeaders: requestHeaders_4_url });
    }
    return (res);
}
function render_grouped_m3u8_urls(urls) {
    let content = document.getElementById('content');

    if (!urls?.length) {
        return set_no_requests(content);
    }

    let trs = [], group_by_av_cnt = 0;
    for (let i = 0, cnt = urls.length; i < cnt; i++) {
        let x = urls[i];
        if (x.is_group_by_audio_video) {
            //x = {
            //    is_group_by_audio_video: true,
            //    video: { url: m3u8_url, requestHeaders: requestHeaders_4_url },
            //    audio: { url: url_a   , requestHeaders: requestHeaders[url_a] || ''}
            //}

            let a = document.createElement('a'); a.className = 'is_group_by_audio_video'; a.title = 'grouped by audio+video';
            a.href = '#';
            a.setAttribute('is_group_by_audio_video', true);
            a.setAttribute('video-url'           , x.video.url);
            a.setAttribute('video-requestHeaders', x.video.requestHeaders);
            a.setAttribute('audio-url'           , x.audio.url);
            a.setAttribute('audio-requestHeaders', x.audio.requestHeaders);
            a.appendChild(document.createTextNode( get_trimmed_url(x.grouped_url_text) ));

            let img = document.createElement('img'); img.src = 'img/group_by_audio_video_1.png'; img.style.height = '16px'; img.title = a.title;
            //a.appendChild(img);
            let td = document.createElement('td');
            td.appendChild(img);

            let td2 = document.createElement('td'); td2.className = 'content'; td2.title = x.grouped_url_text;
            td2.appendChild(a);

            let tr = document.createElement('tr');
            tr.appendChild(td);
            tr.appendChild(td2);

            trs.push(tr.outerHTML);
            group_by_av_cnt++;
        } else {
            //x = { url: m3u8_url, requestHeaders: requestHeaders_4_url }
            trs.push( create_single_m3u8_url__html(x) );

            //trs.push('<tr><td><a class="auto_start_download" title="auto start download" href="' + x.url + '"><img src="img/auto_start_download.png" style="height: 16px"/></a></td>' +
            //         '<td class="content" title="' + x.url + '"><a class="download" href="' + x.url + '">' + x.url + '</a></td></tr>');
        }
    }
    set_content(content, trs, urls.length, group_by_av_cnt);
    append_common_EventListeners(content);

    if (group_by_av_cnt) {
        let aa = content.querySelectorAll('a.is_group_by_audio_video');
        for (let i = 0; i < aa.length; i++) {
            aa[i].addEventListener('click', function (e) {
                send2host_msgObj(create_grouped_msgObj(this));
                e.preventDefault();
                return (false);
            });
        }
    }
}

function render_m3u8_urls(m3u8_urls, requestHeaders) {
    let content = document.getElementById('content');

    if (!m3u8_urls?.length) {
        return set_no_requests(content);
    }

    let trs = [];
    for (let i = 0, cnt = m3u8_urls.length; i < cnt; i++) {
        const m3u8_url = m3u8_urls[i];
        trs.push(create_single_m3u8_url__html({ url: m3u8_url, requestHeaders: requestHeaders[m3u8_url] || '' }));

        //trs.push('<tr><td><a class="auto_start_download" title="auto start download" href="' + m3u8_url + '"><img src="img/auto_start_download.png" style="height: 16px"/></a></td>' +
        //         '<td class="content" title="' + m3u8_url + '"><a class="download" href="' + m3u8_url + '">' + m3u8_url + '</a></td></tr>');
    }
    set_content(content, trs, m3u8_urls.length);
    append_common_EventListeners(content);
}
function set_content(content, trs, urls_count, group_by_av_count) {
    let group_by_av_txt = group_by_av_count ? ' (grouped: ' + group_by_av_count + ')' : '';
    let download_all = '<h5 class="found"><a class="download_all" title="download all" href="#">m3u8 urls: ' + urls_count + group_by_av_txt + '</a></h5>';
    let auto_start_download_all = ((1 < urls_count) ? '<a class="auto_start_download_all" title="auto start download all" href="#"><img src="img/auto_start_download.png" style="height: 16px"/></a>' : '');
    content.innerHTML = '<table><tr><td>' + auto_start_download_all + '</td><td>' + download_all + '</td></tr></table>' +
                        '<table class="content">' + trs.join('') + '</table>';
}
function create_single_m3u8_url__html(x /*x = { url: m3u8_url, requestHeaders: requestHeaders_4_url }*/) {
    let a = document.createElement('a');
    a.className = 'auto_start_download';
    a.title     = 'auto start download';
    a.href      = x.url;
    a.setAttribute('requestHeaders', x.requestHeaders);

    let img = document.createElement('img'); img.src = 'img/auto_start_download.png'; img.style.height = '16px';
    a.appendChild(img);

    let td = document.createElement('td');
    td.appendChild(a);

    let a2 = document.createElement('a'); a2.className = 'download'; a2.href = x.url; a2.setAttribute('requestHeaders', x.requestHeaders);
    a2.appendChild(document.createTextNode( get_trimmed_url(x.url) ));

    let td2 = document.createElement('td'); td2.className = 'content'; td2.title = x.url;
    td2.appendChild(a2);

    let tr = document.createElement('tr');
    tr.appendChild(td);
    tr.appendChild(td2);

    return (tr.outerHTML);
}

function append_common_EventListeners(content) {
    let aa = content.querySelectorAll('a.download');
    for (let i = 0; i < aa.length; i++) {
        aa[i].addEventListener('click', function (e) {
            send2host_msgObj( create_msgObj(this) );
            e.preventDefault();
            return (false);
        });
    }

    aa = content.querySelectorAll('a.auto_start_download');
    for (let i = 0; i < aa.length; i++) {
        aa[i].addEventListener('click', function (e) {
            send2host_msgObj( create_msgObj(this, true) );
            e.preventDefault();
            return (false);
        });
    }

    aa = content.querySelectorAll('a.download_all');
    if (0 < aa.length) {
        aa[0].addEventListener('click', function (e) {
            let msgObjsArray = [], bb = content.querySelectorAll('a.download, a.is_group_by_audio_video');
            for (let j = 0; j < bb.length; j++) {
                const b = bb[j], msgObj = b.getAttribute('is_group_by_audio_video') ? create_grouped_msgObj(b) : create_msgObj(b);
                msgObjsArray.push( msgObj );
            }
            send2host_msgObjsArray(msgObjsArray);
            e.preventDefault();
            return (false);
        });
    }

    aa = content.querySelectorAll('a.auto_start_download_all');
    if (0 < aa.length) {
        aa[0].addEventListener('click', function (e) {
            let msgObjsArray = [], bb = content.querySelectorAll('a.auto_start_download');
            for (let j = 0; j < bb.length; j++) {
                msgObjsArray.push( create_msgObj(bb[j], true) );
            }
            send2host_msgObjsArray(msgObjsArray);
            e.preventDefault();
            return (false);
        });
    }
}
function set_no_requests(content) { content.innerHTML = '<h5 class="not-found">m3u8 requests no were made from this page</h5>'; }

function create_grouped_msgObj(a, auto_start_download) {
    return {
        is_group_by_audio_video: true,
        auto_start_download    : !!auto_start_download,
        video_url              : a.getAttribute('video-url'),
        video_requestHeaders   : a.getAttribute('video-requestHeaders'),
        audio_url              : a.getAttribute('audio-url'),
        audio_requestHeaders   : a.getAttribute('audio-requestHeaders')
    };
}
function create_msgObj(a, auto_start_download) {
    return {
        m3u8_url           : a.href,
        requestHeaders     : a.getAttribute('requestHeaders') || '',
        auto_start_download: !!auto_start_download
    };
}
function send2host_msgObj(msgObj) { send2host_msgObjsArray([msgObj]); }
function send2host_msgObjsArray(msgObjsArray) {
    const HOST_NAME = 'm3u8.downloader.host';

    try {
        root.runtime.sendNativeMessage(
            HOST_NAME,
            { array: msgObjsArray },
            resp => {
                let message;
                if (resp) {
                    if (resp.text === 'success') {
                        console.log('[' + HOST_NAME + '] sent the response: \'' + JSON.stringify(resp) + '\'');
                        return;
                    }
                    message = resp.text || JSON.stringify(resp);
                }
                else if (root.runtime.lastError?.message) {
                    message = root.runtime.lastError.message;
                }

                let notificationOptions = {
                    type    : 'basic',
                    title   : '[' + HOST_NAME + '] => send-native-message ERROR:',
                    message : message || '[NULL]',
                    iconUrl : 'img/m3u8_148.png',
                    priority: 2
                };
                root.notifications.clear(HOST_NAME);
                root.notifications.create(HOST_NAME, notificationOptions);
            });
    }
    catch (ex) {
        let message = ex + '';
        let notificationOptions = {
            type    : 'basic',
            title   : '[' + HOST_NAME + '] => send-native-message ERROR:',
            message : message || '[NULL]',
            iconUrl : 'img/m3u8_148.png',
            priority: 2
        };
        root.notifications.clear(HOST_NAME);
        root.notifications.create(HOST_NAME, notificationOptions);
    }
}
