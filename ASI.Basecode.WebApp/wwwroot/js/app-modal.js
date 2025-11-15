// Lightweight helper to show account-styled modals (alert/confirm) with icons
(function (global) {
    function ensureModal() {
        var existing = document.getElementById('appModal');
        if (existing) { return existing; }
        console.warn('App modal not found in DOM. Include \/Views\/Shared\/_AppModal.cshtml');
        return null;
    }

    function getBsModal(element) {
        if (!element) return null;
        // Bootstrap 5 path
        try {
            if (window.bootstrap && window.bootstrap.Modal) {
                var Modal = window.bootstrap.Modal;
                if (typeof Modal.getOrCreateInstance === 'function') {
                    return Modal.getOrCreateInstance(element);
                }
                // Fallback for older v5 builds
                return new Modal(element);
            }
        } catch (e) { /* fall through */ }

        // Bootstrap 4 path using jQuery plugin
        if (window.jQuery && typeof window.jQuery.fn.modal === 'function') {
            return {
                show: function () { window.jQuery(element).modal('show'); },
                hide: function () { window.jQuery(element).modal('hide'); },
                onHidden: function (handler) { window.jQuery(element).one('hidden.bs.modal', handler); }
            };
        }

        console.warn('Bootstrap Modal API not available (no bootstrap.Modal or jQuery modal).');
        return null;
    }

    function setVariant(variant) {
        var content = document.querySelector('#appModal .app-modal');
        if (!content) return;
        content.classList.remove('is-success', 'is-error', 'is-warning', 'is-info', 'is-confirm');
        if (variant) content.classList.add('is-' + variant);
        var icon = document.getElementById('appModalIcon');
        if (icon) {
            var i = icon.querySelector('i');
            if (i) i.className = '';
            var map = {
                success: 'fas fa-circle-check',
                error: 'fas fa-circle-xmark',
                warning: 'fas fa-triangle-exclamation',
                info: 'fas fa-circle-info',
                confirm: 'fas fa-circle-question'
            };
            if (i) i.className = map[variant] || 'fas fa-circle-info';
        }
    }

    function show(options) {
        var el = ensureModal();
        if (!el) return Promise.resolve(false);
        var modal = getBsModal(el);
        var titleEl = document.getElementById('appModalTitle');
        var msgEl = document.getElementById('appModalMessage');
        var okBtn = document.getElementById('appModalOk');
        var cancelBtn = document.getElementById('appModalCancel');

        titleEl.textContent = options.title || 'Notice';
        if (typeof options.message === 'string') {
            msgEl.textContent = options.message;
        } else {
            msgEl.innerHTML = '';
            msgEl.appendChild(options.message);
        }
        setVariant(options.variant || 'info');

        okBtn.textContent = options.okText || 'OK';
        cancelBtn.textContent = options.cancelText || 'Cancel';
        cancelBtn.style.display = options.showCancel ? '' : 'none';

        return new Promise(function (resolve) {
            var resolved = false;
            function cleanup() {
                okBtn.onclick = null; cancelBtn.onclick = null;
            }
            function onHide() { if (!resolved) resolve(false); cleanup(); }

            okBtn.onclick = function () { resolved = true; resolve(true); modal.hide(); };
            cancelBtn.onclick = function () { resolved = true; resolve(false); modal.hide(); };

            // Hook hidden event for both BS5 and BS4
            if (el.addEventListener) {
                el.addEventListener('hidden.bs.modal', onHide, { once: true });
            }
            if (typeof modal.onHidden === 'function') {
                modal.onHidden(onHide);
            }

            modal.show();
        });
    }

    var api = {
        alert: function (opts) {
            return show({
                title: opts && opts.title,
                message: (opts && opts.message) || '',
                okText: (opts && opts.okText) || 'OK',
                showCancel: false,
                variant: (opts && opts.variant) || 'info'
            });
        },
        confirm: function (opts) {
            return show({
                title: (opts && opts.title) || 'Please confirm',
                message: (opts && opts.message) || '',
                okText: (opts && opts.okText) || 'Confirm',
                cancelText: (opts && opts.cancelText) || 'Cancel',
                showCancel: true,
                variant: (opts && opts.variant) || 'confirm'
            });
        }
    };

    global.appModal = api;
})(window);


