window.onload = async function () {
    //---console.log('start \'m3u8_options.html\': ' + new Date().toLocaleString());

    let res = await chrome.storage.local.get();
    let ch = document.getElementById('saveUrlListBetweenTabReload');
    ch.checked = !!res.saveUrlListBetweenTabReload;
    ch.addEventListener('click', async function (event) {
        //---console.log('this.checked \'m3u8_options.html\': ' + this.checked);

        await chrome.storage.local.set({ saveUrlListBetweenTabReload: this.checked });
    });
};