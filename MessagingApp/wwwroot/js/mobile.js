(function () {
    // Only run on phones/small tablets
    const mm = window.matchMedia('(max-width: 800px)');
    if (!mm.matches) return;

    // Offcanvas: auto-close when user picks a conversation (any link/tappable row)
    const drawerEl = document.getElementById('convoDrawer');
    if (drawerEl && window.bootstrap) {
        const drawer = bootstrap.Offcanvas.getOrCreateInstance(drawerEl);
        drawerEl.addEventListener('click', (e) => {
            const item = e.target.closest('a, button, .user-container, .list-group-item');
            if (!item) return;
            // Defer hide to allow navigation handlers to run first
            setTimeout(() => drawer.hide(), 0);
        });
    }

    // Keep the composer visible when the keyboard pops up
    const ta = document.getElementById('contentInput');
    if (ta) {
        ta.addEventListener('focus', () => {
            setTimeout(() => ta.scrollIntoView({ block: 'nearest' }), 120);
        });
    }

    document.querySelectorAll('.message-actions i').forEach((el) => {
        el.style.padding = el.style.padding || '10px';
    });
})();

