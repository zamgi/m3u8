const root = browser; //chrome;
window.addEventListener('load', async function () {
    const set_def_val = (d, prop, defVal) => { if (d[prop] === undefined) d[prop] = defVal; };
    let opt = await root.storage.local.get(); if (!opt) opt = {}; set_def_val(opt, 'groupByAudioVideo', true);
                                                                  set_def_val(opt, 'saveUrlListBetweenTabReload', false);
                                                                  set_def_val(opt, 'enableButtonEvenIfNoUrls'   , false);

    const set_checkbox_click_handler = (checkboxId, opt_prop) => {
        if (!opt_prop) opt_prop = checkboxId;

        const ch = document.getElementById(checkboxId);
        ch.checked = !!opt[opt_prop];
        ch.addEventListener('click', async function () {
            opt[opt_prop] = this.checked;
            await root.storage.local.set(opt);
        });
    },
    set_checkbox_click_handler_revert = (checkboxId, opt_prop) => {
        if (!opt_prop) opt_prop = checkboxId;

        const ch = document.getElementById(checkboxId);
        ch.checked = !opt[opt_prop];
        ch.addEventListener('click', async function () {
            opt[opt_prop] = !this.checked;
            await root.storage.local.set(opt);
        });
    };
    set_checkbox_click_handler('groupByAudioVideo');
    set_checkbox_click_handler('saveUrlListBetweenTabReload');    
    set_checkbox_click_handler('enableButtonEvenIfNoUrls');
    set_checkbox_click_handler_revert('showSingleUrls', 'hideSingleUrls');
    set_checkbox_click_handler_revert('showGroupedUrls', 'hideGroupedUrls');
});