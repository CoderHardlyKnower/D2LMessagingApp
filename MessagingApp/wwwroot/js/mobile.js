/* ============================================================================
Mobile chat keyboard/composer ergonomics (≤800px)
- Sticky composer that rides above the soft keyboard (visualViewport-powered)
- Message list bottom padding equals live composer height
- Auto-grow textarea; Enter to send (no Shift)
- Auto-scroll to newest only when user is near the newest edge
- No DOM moves, no desktop changes, no structural churn
============================================================================ */

(function () {
    const isMobile = window.matchMedia('(max-width: 800px)').matches;
    if (!isMobile) return;

    const $ = (s, r = document) => r.querySelector(s);
    const $$ = (s, r = document) => Array.from(r.querySelectorAll(s));

    const list = $('#messageList') || $('.message-list');
    const form = $('#messageForm');
    const textarea = $('#contentInput');

    /* ---------------------------- CSS var helpers ---------------------------- */
    const setVar = (name, value) => document.documentElement.style.setProperty(name, value);

    /* --------------------------- Composer measurement ------------------------ */
    function measureComposer() {
        if (!form) return;
        const rect = form.getBoundingClientRect();
        const h = Math.round(rect.height);
        if (h > 0) setVar('--composerH', h + 'px');
    }

    /* ------------------------- visualViewport handling ----------------------- */
    function isEditableFocused() {
        const ae = document.activeElement;
        if (!ae) return false;
        const tag = ae.tagName;
        if (tag === 'TEXTAREA') return true;
        if (tag === 'INPUT') {
            const t = (ae.type || '').toLowerCase();
            return !['button', 'submit', 'checkbox', 'radio', 'file', 'color', 'range', 'reset'].includes(t);
        }
        return !!ae.isContentEditable;
    }

    function bindVisualViewport() {
        const vv = window.visualViewport;

        function update() {
            const vvh = vv ? vv.height : window.innerHeight;
            const lift = vv ? Math.max(0, window.innerHeight - vv.height) : 0;

            setVar('--vvh', vvh + 'px');
            setVar('--kb-offset', lift + 'px');

            const kbOpen = lift > 0 || (isEditableFocused() && (window.innerHeight - vvh) > 80);
            document.body.classList.toggle('kb-open', kbOpen);

            // composer height can change (auto-grow), keep vars fresh
            measureComposer();
        }

        if (vv) {
            vv.addEventListener('resize', update);
            vv.addEventListener('scroll', update); // iOS reports kb via scroll
            update();
        } else {
            // Fallback: focus/blur + resize
            const fallback = () => {
                setVar('--vvh', window.innerHeight + 'px');
                const open = isEditableFocused();
                document.body.classList.toggle('kb-open', open);
                measureComposer();
            };
            window.addEventListener('resize', fallback);
            document.addEventListener('focusin', fallback);
            document.addEventListener('focusout', fallback);
            fallback();
        }
    }

    /* --------------------------- Textarea auto-grow -------------------------- */
    function bindTextarea() {
        if (!textarea) return;

        const lineH = parseFloat(getComputedStyle(textarea).lineHeight) || 20;
        const maxH = Math.round(lineH * 6.5); // ~6 lines before internal scroll

        function grow() {
            textarea.style.height = 'auto';
            const next = Math.min(textarea.scrollHeight, maxH);
            textarea.style.height = next + 'px';
            textarea.style.overflowY = textarea.scrollHeight > next ? 'auto' : 'hidden';
            // composer height changes with textarea height
            measureComposer();
        }

        textarea.addEventListener('input', grow);

        textarea.addEventListener('focus', () => {
            // ensure caret is visible after keyboard animates
            setTimeout(() => textarea.scrollIntoView({ block: 'nearest' }), 140);
        });

        // Enter to send (no Shift)
        textarea.addEventListener('keydown', (e) => {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                form?.dispatchEvent(new Event('submit', { cancelable: true }));
            }
        });

        // Initial sizing
        grow();
    }

    /* ------------------------- Newest-edge auto-scroll ----------------------- */
    function isNearNewest() {
        if (!list) return true;
        // With column-reverse, 0 means we're at the newest end.
        return list.scrollTop <= 8;
    }

    function scrollToNewest(smooth = true) {
        if (!list) return;
        list.scrollTo({ top: 0, behavior: smooth ? 'smooth' : 'auto' });
    }

    function bindMutationAutoScroll() {
        if (!list) return;
        const mo = new MutationObserver(() => {
            if (isNearNewest()) {
                // Give images a tick to layout before scrolling
                setTimeout(() => scrollToNewest(true), 10);
            }
        });
        mo.observe(list, { childList: true });
    }

    /* ------------------------- Tap-outside dismiss kb ------------------------ */
    function bindBackgroundBlur() {
        document.addEventListener('touchstart', (e) => {
            if (!textarea) return;
            const inside = e.target.closest('.add-message-form');
            if (!inside && document.activeElement === textarea) textarea.blur();
        }, { passive: true });
    }

    /* --------------------------------- Boot --------------------------------- */
    function init() {
        bindVisualViewport();
        bindTextarea();
        bindMutationAutoScroll();
        bindBackgroundBlur();

        // Re-measure composer on load and when images expand message bubbles
        window.addEventListener('load', measureComposer);
        document.addEventListener('DOMContentLoaded', measureComposer);
        $$('img').forEach(img => {
            if (!img.complete) img.addEventListener('load', measureComposer, { once: true });
        });

        // If your file preview changes composer height, watch it
        const preview = $('#filePreview');
        if (preview) {
            const po = new MutationObserver(measureComposer);
            po.observe(preview, { childList: true, subtree: true });
        }

        // Initial scroll to newest
        scrollToNewest(false);

        // Ensure touch targets on action icons are comfy
        $$('.message-actions i').forEach(el => {
            if (!el.style.padding) el.style.padding = '10px';
        });

        // First measurement
        measureComposer();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();


/* ============================================================================
Keyboard-aware composer (mobile <=800px)
- Adds/removes body.kb-open based on keyboard presence via visualViewport.
- Positions composer using bottom: var(--kb-offset) (no transforms).
- Pads list using --composerH so newest message never hides.
- Does NOT clamp body height (prevents large whitespace/jumps).
============================================================================ */

(function () {
    const isMobile = window.matchMedia('(max-width: 800px)').matches;
    if (!isMobile) return;

    const $ = (s, r = document) => r.querySelector(s);
    const form = $('#messageForm');
    const list = $('#messageList') || document.querySelector('.message-list');
    const textarea = $('#contentInput');

    const setVar = (n, v) => document.documentElement.style.setProperty(n, v);

    function measureComposer() {
        if (!form) return;
        const h = Math.round(form.getBoundingClientRect().height);
        if (h > 0) setVar('--composerH', h + 'px');
    }

    function isEditableFocused() {
        const ae = document.activeElement;
        if (!ae) return false;
        const tag = ae.tagName;
        if (tag === 'TEXTAREA') return true;
        if (tag === 'INPUT') {
            const t = (ae.type || '').toLowerCase();
            return !['button', 'submit', 'checkbox', 'radio', 'file', 'color', 'range', 'reset'].includes(t);
        }
        return !!ae.isContentEditable;
    }

    function bindVisualViewport() {
        const vv = window.visualViewport;

        function update() {
            // How much the OSK overlaps the layout viewport.
            // We purposely do NOT also clamp body height; we only offset the composer.
            const overlap = vv ? Math.max(0, window.innerHeight - vv.height) : 0;
            setVar('--kb-offset', overlap + 'px');

            const kbOpen = overlap > 0 || (isEditableFocused() && (window.innerHeight - (vv?.height ?? window.innerHeight)) > 80);
            document.body.classList.toggle('kb-open', kbOpen);

            // Keep composer height accurate (textarea can auto-grow, previews can add)
            measureComposer();
        }

        if (vv) {
            vv.addEventListener('resize', update);
            vv.addEventListener('scroll', update); // iOS sometimes signals kb via scroll
            update();
        } else {
            // Fallback sans visualViewport
            const fallback = () => {
                const open = isEditableFocused();
                document.body.classList.toggle('kb-open', open);
                setVar('--kb-offset', open ? '320px' : '0px'); // conservative guess
                measureComposer();
            };
            window.addEventListener('resize', fallback);
            document.addEventListener('focusin', fallback);
            document.addEventListener('focusout', fallback);
            fallback();
        }
    }

    function bindTextareaAssist() {
        if (!textarea) return;
        textarea.addEventListener('focus', () => {
            // After the keyboard animates, ensure caret is visible
            setTimeout(() => textarea.scrollIntoView({ block: 'nearest' }), 140);
        });
        textarea.addEventListener('input', () => {
            // If your app auto-grows, re-measure composer height next tick
            requestAnimationFrame(measureComposer);
        });
    }

    function init() {
        bindVisualViewport();
        bindTextareaAssist();

        // Re-measure when bubbles expand or on load
        window.addEventListener('load', measureComposer);
        document.addEventListener('DOMContentLoaded', measureComposer);

        // If a preview area exists, changes can affect composer height
        const preview = $('#filePreview');
        if (preview) {
            const mo = new MutationObserver(measureComposer);
            mo.observe(preview, { childList: true, subtree: true });
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
