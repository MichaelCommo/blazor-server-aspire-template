window.sidebarInterop = {
    init: function () {
        var isCollapsed = localStorage.getItem('sidebar-collapsed') === 'true';
        document.documentElement.classList.toggle('sidebar-collapsed', isCollapsed);

        var sidebar = document.querySelector('.sidebar');
        var content = document.querySelector('.sidebar-content');
        if (isCollapsed && sidebar) sidebar.classList.add('collapsed');
        if (isCollapsed && content) content.classList.add('collapsed');

        if (sidebar && !sidebar.classList.contains('transitions-ready')) {
            requestAnimationFrame(function () {
                sidebar.classList.add('transitions-ready');
            });
        }

        this.closeMobileNav();
    },
    toggle: function () {
        var isCollapsed = document.documentElement.classList.toggle('sidebar-collapsed');
        localStorage.setItem('sidebar-collapsed', isCollapsed);

        var sidebar = document.querySelector('.sidebar');
        var content = document.querySelector('.sidebar-content');
        if (sidebar) {
            sidebar.classList.toggle('collapsed', isCollapsed);
            sidebar.classList.add('transitions-ready');
        }
        if (content) content.classList.toggle('collapsed', isCollapsed);
    },
    toggleMobileNav: function () {
        var nav = document.querySelector('.nav-scrollable');
        if (nav) nav.classList.toggle('show');
    },
    closeMobileNav: function () {
        var nav = document.querySelector('.nav-scrollable');
        if (nav) nav.classList.remove('show');
    }
};
