// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener('DOMContentLoaded', () => {
    const sidebarToggle = document.getElementById('sidebarToggle');
    const adminWrapper = document.querySelector('.admin-wrapper');

    if (sidebarToggle && adminWrapper) {
        // Restore collapse state
        const isCollapsed = localStorage.getItem('admin_sidebar_collapsed') === 'true';
        if (isCollapsed) {
            adminWrapper.classList.add('sidebar-collapsed');
        }

        sidebarToggle.addEventListener('click', () => {
            adminWrapper.classList.toggle('sidebar-collapsed');
            localStorage.setItem('admin_sidebar_collapsed', adminWrapper.classList.contains('sidebar-collapsed'));
        });
    }
});
