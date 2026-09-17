$(document).ready(function () {
    // Sidebar toggle for mobile
    $('.sidebar-toggle').on('click', function () {
        $('.sidebar').toggleClass('show');
    });

    // Close sidebar when clicking outside on mobile
    $(document).on('click', function (e) {
        if ($(window).width() <= 768) {
            if (!$(e.target).closest('.sidebar').length && !$(e.target).closest('.sidebar-toggle').length) {
                $('.sidebar').removeClass('show');
            }
        }
    });

    // Auto-hide alerts after 5 seconds
    setTimeout(function () {
        $('.alert-dismissible').alert('close');
    }, 5000);

    // Load notification count
    loadNotificationCount();
    setInterval(loadNotificationCount, 30000);

    // Load notifications on dropdown open
    $('#notificationDropdownBtn').on('click', function () {
        loadNotifications();
    });

    // Mark all read
    $(document).on('click', '#markAllRead', function () {
        $.post('/Notification/MarkAllAsRead', function () {
            loadNotificationCount();
            loadNotifications();
        });
    });
});

function loadNotificationCount() {
    $.get('/Notification/GetUnreadCount', function (data) {
        var badge = $('.notification-badge');
        if (data.count > 0) {
            badge.text(data.count).show();
        } else {
            badge.hide();
        }
    });
}

function loadNotifications() {
    $.get('/Notification/GetRecent', function (html) {
        $('.notification-list').html(html);
    });
}
