window.addEventListener('load', async function (/*event*/) {
    let res = await chrome.storage.local.get();

    let ch = document.getElementById('saveUrlListBetweenTabReload');
    ch.checked = !!res.saveUrlListBetweenTabReload;
    ch.addEventListener('click', async function (/*event*/) {
        await chrome.storage.local.set({ saveUrlListBetweenTabReload: this.checked });
    });

    ch = document.getElementById('directionRtl');
    ch.checked = (res.directionRtl !== undefined) ? !!res.directionRtl : ch.checked;
    ch.addEventListener('click', async function (/*event*/) {
        await chrome.storage.local.set({ directionRtl: this.checked });
    });
});