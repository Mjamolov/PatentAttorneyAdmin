(function () {
    function setupSidebarToggle(toggleId, sidebarSelector) {
        var toggle = document.getElementById(toggleId);
        var sidebar = document.querySelector(sidebarSelector);
        if (!toggle || !sidebar) return;

        var overlay = document.querySelector('.sidebar-overlay');
        if (!overlay) {
            overlay = document.createElement('div');
            overlay.className = 'sidebar-overlay';
            overlay.setAttribute('aria-hidden', 'true');
            document.body.appendChild(overlay);
        }

        function close() {
            sidebar.classList.remove('open');
            overlay.classList.remove('visible');
            toggle.setAttribute('aria-expanded', 'false');
            document.body.classList.remove('nav-open');
        }

        function open() {
            sidebar.classList.add('open');
            overlay.classList.add('visible');
            toggle.setAttribute('aria-expanded', 'true');
            document.body.classList.add('nav-open');
        }

        toggle.addEventListener('click', function () {
            if (sidebar.classList.contains('open')) close();
            else open();
        });

        overlay.addEventListener('click', close);

        sidebar.querySelectorAll('nav a').forEach(function (link) {
            link.addEventListener('click', close);
        });

        window.addEventListener('resize', function () {
            if (window.innerWidth > 768) close();
        });
    }

    setupSidebarToggle('clientNavToggle', '.client-sidebar');
    setupSidebarToggle('adminNavToggle', '.admin-sidebar');

    var siteToggle = document.getElementById('siteNavToggle');
    var siteNav = document.getElementById('siteMobileNav');
    if (siteToggle && siteNav) {
        siteToggle.addEventListener('click', function () {
            var isOpen = siteNav.classList.toggle('open');
            siteToggle.setAttribute('aria-expanded', isOpen ? 'true' : 'false');
            document.body.classList.toggle('nav-open', isOpen);
        });

        siteNav.querySelectorAll('a').forEach(function (link) {
            link.addEventListener('click', function () {
                siteNav.classList.remove('open');
                siteToggle.setAttribute('aria-expanded', 'false');
                document.body.classList.remove('nav-open');
            });
        });

        window.addEventListener('resize', function () {
            if (window.innerWidth > 768) {
                siteNav.classList.remove('open');
                siteToggle.setAttribute('aria-expanded', 'false');
                document.body.classList.remove('nav-open');
            }
        });
    }
})();
