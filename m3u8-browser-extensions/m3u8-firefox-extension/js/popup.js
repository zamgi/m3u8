const root = browser; //chrome;
window.addEventListener('load', async function () {
    const getActiveTabId = async () => {
        // Query for the active tab in the current window
        const tabs = await root.tabs.query({ active: true, currentWindow: true });
        return (tabs?.length ? tabs[0].id : undefined);
    }

    try {
        // get m3u8 urls for current active tab
        const backgroundPage = root.extension.getBackgroundPage();
        const activeTabId = await getActiveTabId();
        const tabInfo = backgroundPage.workInfo.getM3u8UrlsByActiveTabId(activeTabId);

        const set_def_val = (d, prop, defVal) => { if (d[prop] === undefined) d[prop] = defVal; };
        let opt = await root.storage.local.get(); if (!opt) opt = {}; set_def_val(opt, 'groupByAudioVideo', true);
                                                                      //set_def_val(opt, 'directionRtl'     , true);
                                                                      set_def_val(opt, 'saveUrlListBetweenTabReload', false);

        const set_checkbox_click_handler = (checkboxId, onClickFunc, opt_prop) => {
            if (!opt_prop) opt_prop = checkboxId;

            const ch = document.getElementById(checkboxId);
            ch.checked = !!opt[opt_prop];
            if (onClickFunc) onClickFunc(ch.checked);
            ch.addEventListener('click', async function () {                
                opt[opt_prop] = this.checked;
                await root.storage.local.set(opt);
                if (onClickFunc) onClickFunc(this.checked);
            });
        };

        const clearUrlListFunc = async () => {
            await backgroundPage.workInfo.deleteAllUrlsFromTab(activeTabId);

            tabInfo.m3u8_urls = tabInfo.requestHeaders = null;
            render_m3u8_urls();
        },
        deleteUrlFromListFunc = async d => {
            if (d.is_group_by_audio_video) {
                await backgroundPage.workInfo.deleteUrlFromTab(activeTabId, d.video_url); 
                await backgroundPage.workInfo.deleteUrlFromTab(activeTabId, d.audio_url); 
            }
            else {
                await backgroundPage.workInfo.deleteUrlFromTab(activeTabId, d.m3u8_url); 
            }
            renderFunc(opt.groupByAudioVideo);
        },
        clearSingleUrlsFunc = async () => {
            const aa = content.querySelectorAll('a.download');
            if (aa.length) {
                for (let i = 0; i < aa.length; i++) {
                    const d = create_msgObj(aa[i]);
                    await backgroundPage.workInfo.deleteUrlFromTab(activeTabId, d.m3u8_url); 
                }
                renderFunc(opt.groupByAudioVideo);
            }
        },
        clearGroupedUrlsFunc= async () => {
            const aa = content.querySelectorAll('a.is_group_by_audio_video');
            if (aa.length) {
                for (let i = 0; i < aa.length; i++) {
                    const d = create_grouped_msgObj(aa[i]);
                    await backgroundPage.workInfo.deleteUrlFromTab(activeTabId, d.video_url);
                    await backgroundPage.workInfo.deleteUrlFromTab(activeTabId, d.audio_url); 
                }
                renderFunc(opt.groupByAudioVideo);
            }
        };

        const handlers = { clearUrlListFunc, deleteUrlFromListFunc, clearSingleUrlsFunc, clearGroupedUrlsFunc };
        const renderFunc = groupByAudioVideo => {
            if (groupByAudioVideo) {
                let urls = group_by_audio_video(tabInfo);
                render_grouped_m3u8_urls(urls, handlers);
            } else {
                render_m3u8_urls(tabInfo, handlers);
            }
        }

        set_checkbox_click_handler('groupByAudioVideo', renderFunc);
        //set_checkbox_click_handler('directionRtl', rtl => document.getElementById('content').style.direction = rtl ? 'rtl' : '');
        set_checkbox_click_handler('saveUrlListBetweenTabReload');

    } catch (ex) {
        console.error('m3u8:popup => ' + ex);
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
function group_by_audio_video(tabInfo) {
    const { m3u8_urls, requestHeaders } = tabInfo || {};
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

        res.push({ url: m3u8_url, requestHeaders: requestHeaders_4_url });
    }
    return (res);
}
function render_grouped_m3u8_urls(urls, handlers) {
    const content = document.getElementById('content');

    if (!urls?.length) return set_no_requests(content);

    let trs = [], group_by_av_cnt = 0;
    for (let i = 0, cnt = urls.length; i < cnt; i++) {
        const x = urls[i];
        if (x.is_group_by_audio_video) {
            //x = {
            //    is_group_by_audio_video: true,
            //    video: { url: m3u8_url, requestHeaders: requestHeaders_4_url },
            //    audio: { url: url_a   , requestHeaders: requestHeaders[url_a] || ''}
            //}

            trs.push( create_grouped_m3u8_url__html(x) );
            group_by_av_cnt++;
        }
        else {
            //x = { url: m3u8_url, requestHeaders: requestHeaders_4_url }
            trs.push( create_single_m3u8_url__html(x) );
        }
    }
    set_content(content, trs, urls.length + group_by_av_cnt, group_by_av_cnt);
    append_common_EventListeners(content, handlers);

    if (group_by_av_cnt) {
        const aa = content.querySelectorAll('a.is_group_by_audio_video');
        for (let i = 0; i < aa.length; i++) {
            aa[i].addEventListener('click', function (e) {
                send2host_msgObj(create_grouped_msgObj(this));
                e.preventDefault();
                return (false);
            });
        }
    }
}

function render_m3u8_urls(tabInfo, handlers) {
    let content = document.getElementById('content');

    const { m3u8_urls, requestHeaders } = tabInfo || {};
    if (!m3u8_urls?.length) return set_no_requests(content);

    let trs = [];
    for (let i = 0, cnt = m3u8_urls.length; i < cnt; i++) {
        const m3u8_url = m3u8_urls[i];
        trs.push(create_single_m3u8_url__html({ url: m3u8_url, requestHeaders: requestHeaders[m3u8_url] || '' }));
    }
    set_content(content, trs, m3u8_urls.length);
    append_common_EventListeners(content, handlers);
}
function set_content(content, trs, urls_count, group_by_av_count) {
    let clearUrlList            = '<td> <a id="clearUrlListButton" class="delete_all" title="clear all" href="#"><img src="img/delete_all.png" /></a> </td>';    
    let auto_start_download_all = '<td> <a class="auto_start_download_all" title="auto start download all' + 
                                                      (group_by_av_count ? ' single/non-grouped' : '') + '" href="#"><img src="img/auto_start_download.png" /></a> </td>';
    let caption_total_count     = '<td> <h5 class="found"><a class="download_all" title="download all" href="#">total: ' + urls_count + '</a></h5> </td>';

    let caption_grouped_count = '';
    if (group_by_av_count) {
        let non_grouped_count = (urls_count - 2 * group_by_av_count), non_grouped = '';
        if (non_grouped_count) {
            non_grouped = '<td> <a id="clearSingleUrlsButton" class="delete_all" title="delete all single/non-grouped" href="#"><img src="img/delete_all.png" /></a> </td>' + 
                          '<td> <h5 class="found"><a class="download_all_singles" title="download all single/non-grouped" href="#">singles: ' + non_grouped_count + '</a></h5> </td>';
        }
        let grouped     = '<td> <a id="clearGroupedUrlsButton" class="delete_all" title="delete all grouped" href="#"><img src="img/delete_all.png" /></a> </td>' +
                          '<td> <h5 class="found"><a class="download_all_grouped" title="download all grouped" href="#">grouped: ' + group_by_av_count + '/(' + (2*group_by_av_count) + ')</a></h5> </td>';
        caption_grouped_count = grouped + non_grouped;
    }

    content.innerHTML   = '<table><tr>' + clearUrlList + auto_start_download_all + caption_total_count + caption_grouped_count + '</tr></table>' +
                          '<table class="content">' + trs.join('') + '</table>';
}
function create_single_m3u8_url__html(x /*x = { url: m3u8_url, requestHeaders: requestHeaders_4_url }*/) {
    //1.
    let a1 = document.createElement('a');
    a1.className = 'delete';
    a1.title     = 'delete url';
    a1.href      = '#';
    a1.setAttribute('m3u8_url', x.url);

    let img1 = document.createElement('img'); img1.src = 'img/delete.png'; //img1.style.height = '16px';
    a1.appendChild(img1);

    let td1 = document.createElement('td');
    td1.appendChild(a1);

    //2.
    let a2 = document.createElement('a');
    a2.className = 'auto_start_download';
    a2.title     = 'auto start download';
    a2.href      = x.url;
    a2.setAttribute('requestHeaders', x.requestHeaders);

    let img2 = document.createElement('img'); img2.src = 'img/auto_start_download.png'; //img2.style.height = '16px';
    a2.appendChild(img2);

    let td2 = document.createElement('td');
    td2.appendChild(a2);

    //3.
    let a3 = document.createElement('a'); a3.className = 'download'; a3.href = x.url; a3.setAttribute('requestHeaders', x.requestHeaders);
    a3.appendChild(document.createTextNode( get_trimmed_url(x.url) ));

    let td3 = document.createElement('td'); td3.className = 'content'; td3.title = x.url;
    td3.appendChild(a3);

    //fin.
    let tr = document.createElement('tr');
    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);

    return (tr.outerHTML);

    //'<tr>' +
    //    '<td><a class="delete" title="delete url" href="#"><img src="img/delete.png" /></a></td>' +
    //    '<td><a class="auto_start_download" title="auto start download" href="' + m3u8_url + '"><img src="img/auto_start_download.png" /></a></td>' +
    //    '<td class="content" title="' + m3u8_url + '"><a class="download" href="' + m3u8_url + '">' + m3u8_url + '</a></td>' + 
    //'</tr>'
}
function create_grouped_m3u8_url__html(x) {
    //x = {
    //    is_group_by_audio_video: true,
    //    video: { url: m3u8_url, requestHeaders: requestHeaders_4_url },
    //    audio: { url: url_a   , requestHeaders: requestHeaders[url_a] || ''}
    //}

    //1.
    let a1 = document.createElement('a');
    a1.className = 'delete';
    a1.title     = 'delete url';
    a1.href      = '#';
    a1.setAttribute('is_group_by_audio_video', true);
    a1.setAttribute('video-url', x.video.url);
    a1.setAttribute('audio-url', x.audio.url);

    let img1 = document.createElement('img'); img1.src = 'img/delete.png'; //img1.style.height = '16px';
    a1.appendChild(img1);

    let td1 = document.createElement('td');
    td1.appendChild(a1);

    //2.
    let img2 = document.createElement('img'); img2.src = 'img/group_by_audio_video_1.png'; //img2.style.height = '16px';
    let td2  = document.createElement('td');
    td2.appendChild(img2);


    //3.
    let a3 = document.createElement('a'); a3.className = 'is_group_by_audio_video'; a3.title = 'grouped by audio+video'; img2.title = a3.title;
    a3.href = '#';
    a3.setAttribute('is_group_by_audio_video', true);
    a3.setAttribute('video-url'           , x.video.url);
    a3.setAttribute('video-requestHeaders', x.video.requestHeaders);
    a3.setAttribute('audio-url'           , x.audio.url);
    a3.setAttribute('audio-requestHeaders', x.audio.requestHeaders);
    a3.appendChild(document.createTextNode(get_trimmed_url(x.grouped_url_text)));

    let td3 = document.createElement('td'); td3.className = 'content'; td3.title = x.grouped_url_text;
    td3.appendChild(a3);

    //fin.
    let tr = document.createElement('tr');
    tr.appendChild(td1);
    tr.appendChild(td2);
    tr.appendChild(td3);

    return (tr.outerHTML);
}

function append_common_EventListeners(content, handlers) {
    const set_click_handler__4_send2host_msgObj = (querySelector, auto_start_download) => {
        const aa = content.querySelectorAll(querySelector);
        for (let i = 0; i < aa.length; i++) {
            aa[i].addEventListener('click', function (e) {
                send2host_msgObj(create_msgObj(this, !!auto_start_download));
                e.preventDefault();
                return (false);
            });
        }
    };

    set_click_handler__4_send2host_msgObj('a.download');
    set_click_handler__4_send2host_msgObj('a.auto_start_download', true);

    const set_click_handler__4_send2host_msgObjsArray = (querySelector, querySelector_2, create_msgObj_Func) => {
        const aa = content.querySelectorAll(querySelector);
        if (0 < aa.length) {
            aa[0].addEventListener('click', function (e) {
                const msgObjsArray = [], bb = content.querySelectorAll(querySelector_2);
                for (let j = 0; j < bb.length; j++) {
                    msgObjsArray.push( create_msgObj_Func( bb[ j ] ) );
                }
                send2host_msgObjsArray(msgObjsArray);
                e.preventDefault();
                return (false);
            });
        }
    };

    set_click_handler__4_send2host_msgObjsArray('a.download_all', 'a.download, a.is_group_by_audio_video', a => a.getAttribute('is_group_by_audio_video') ? create_grouped_msgObj(a) : create_msgObj(a));
    set_click_handler__4_send2host_msgObjsArray('a.auto_start_download_all', 'a.auto_start_download', a => create_msgObj(a, true));

    set_click_handler__4_send2host_msgObjsArray('a.download_all_singles', 'a.download', a => create_msgObj(a));
    set_click_handler__4_send2host_msgObjsArray('a.download_all_grouped', 'a.is_group_by_audio_video', a => create_grouped_msgObj(a));

    document.getElementById('clearUrlListButton')?.addEventListener('click', handlers.clearUrlListFunc);
    document.getElementById('clearSingleUrlsButton')?.addEventListener('click', handlers.clearSingleUrlsFunc);
    document.getElementById('clearGroupedUrlsButton')?.addEventListener('click', handlers.clearGroupedUrlsFunc);

    const aa = content.querySelectorAll('a.delete');
    for (let i = 0; i < aa.length; i++) {
        aa[i].addEventListener('click', function (e) {
            const d = this.getAttribute('is_group_by_audio_video')
                ? { is_group_by_audio_video: true, video_url: this.getAttribute('video-url'), audio_url: this.getAttribute('audio-url') }
                : { m3u8_url: this.getAttribute('m3u8_url') };
            handlers.deleteUrlFromListFunc(d);
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
        video_requestHeaders   : a.getAttribute('video-requestHeaders') || '',
        audio_url              : a.getAttribute('audio-url'),
        audio_requestHeaders   : a.getAttribute('audio-requestHeaders') || ''
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
