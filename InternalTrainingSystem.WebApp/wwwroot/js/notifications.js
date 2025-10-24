// Notification System
let notificationCount = 0;
let currentPage = 1;
const pageSize = 10;

function loadNotifications(page = 1) {
    const apiUrl = `/Notification/GetUserNotifications?page=${page}&pageSize=${pageSize}`;
    
    fetch(apiUrl)
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(data => {
            console.log('Notifications loaded:', data);
            if (data.success) {
                updateNotificationBadge(data.unreadCount);
                displayNotifications(data.notifications, page === 1);
                currentPage = page;
            } else {
                console.error('Error loading notifications:', data.message);
                showEmptyNotifications();
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showEmptyNotifications();
        });
}

function updateNotificationBadge(count) {
    const badge = document.getElementById('notification-badge');
    if (badge) {
        if (count > 0) {
            badge.textContent = count > 99 ? '99+' : count;
            badge.style.display = 'inline';
        } else {
            badge.style.display = 'none';
        }
        notificationCount = count;
    }
}

function displayNotifications(notifications, clearList = true) {
    const notificationList = document.getElementById('notification-list');
    
    if (!notificationList) {
        console.error('Notification list element not found');
        return;
    }
    
    if (!notifications || notifications.length === 0) {
        showEmptyNotifications();
        return;
    }
    
    let html = '';
    notifications.forEach(notification => {
        const isUnread = !notification.isRead ? 'notification-unread' : '';
        const timeAgo = formatTimeAgo(notification.createdAt);
        
        html += `
            <li class="notification-item ${isUnread}" data-id="${notification.id}">
                <a class="dropdown-item notification-link" href="${notification.link || '#'}" onclick="markAsRead(${notification.id}, event); return false;">
                    <div class="d-flex align-items-start p-2">
                        <div class="notification-icon me-3 mt-1">
                            <i class="${notification.icon}"></i>
                        </div>
                        <div class="notification-content flex-grow-1">
                            <h6 class="notification-title mb-1">${notification.title}</h6>
                            <p class="notification-message mb-2">${notification.message}</p>
                            <small class="notification-time text-muted">
                                <i class="fas fa-clock me-1"></i>${timeAgo}
                            </small>
                        </div>
                        ${!notification.isRead ? '<div class="notification-dot"></div>' : ''}
                    </div>
                </a>
            </li>
        `;
    });
    
    if (clearList) {
        notificationList.innerHTML = html;
    } else {
        notificationList.innerHTML += html;
    }
}

function showEmptyNotifications() {
    const notificationList = document.getElementById('notification-list');
    if (notificationList) {
        notificationList.innerHTML = `
            <li class="text-center p-4">
                <i class="fas fa-bell-slash text-muted fs-2 mb-2"></i>
                <div class="text-muted">Không có thông báo nào</div>
            </li>
        `;
    }
}

function markAsRead(notificationId, event) {
    if (event) {
        event.preventDefault();
        event.stopPropagation();
    }
    
    fetch('/Notification/MarkAsRead', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ notificationId: notificationId })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            // Update UI
            const notificationItem = document.querySelector(`[data-id="${notificationId}"]`);
            if (notificationItem) {
                notificationItem.classList.remove('notification-unread');
                const dot = notificationItem.querySelector('.notification-dot');
                if (dot) dot.remove();
            }
            
            // Update badge
            if (notificationCount > 0) {
                updateNotificationBadge(notificationCount - 1);
            }
            
            // Navigate to link if exists
            const link = notificationItem?.querySelector('.notification-link')?.getAttribute('href');
            if (link && link !== '#') {
                window.location.href = link;
            }
        }
    })
    .catch(error => {
        console.error('Error marking notification as read:', error);
    });
}

function markAllAsRead() {
    fetch('/Notification/MarkAllAsRead', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        }
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            // Update UI
            document.querySelectorAll('.notification-item').forEach(item => {
                item.classList.remove('notification-unread');
                const dot = item.querySelector('.notification-dot');
                if (dot) dot.remove();
            });
            updateNotificationBadge(0);
            
            // Show success message
            showToast('Đã đánh dấu tất cả thông báo', 'success');
        }
    })
    .catch(error => {
        console.error('Error marking all notifications as read:', error);
        showToast('Có lỗi xảy ra', 'error');
    });
}

function loadMoreNotifications() {
    // Close dropdown and navigate to full notifications page
    const dropdown = document.getElementById('notificationDropdown');
    if (dropdown) {
        const bsDropdown = bootstrap.Dropdown.getInstance(dropdown);
        if (bsDropdown) {
            bsDropdown.hide();
        }
    }
    
    // In a real app, this would navigate to a full notifications page
    alert('Tính năng xem tất cả thông báo sẽ được triển khai sau');
}

function createTestNotification() {
    const types = ['start', 'reminder', 'finish', 'certificate', 'staffconfirm'];
    const randomType = types[Math.floor(Math.random() * types.length)];
    
    fetch('/Notification/CreateSampleNotification', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ type: randomType })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            showToast('Tạo thông báo mới thành công!', 'success');
            // Reload notifications to show the new one
            setTimeout(() => {
                loadNotifications();
            }, 500);
        } else {
            showToast('Lỗi khi tạo thông báo: ' + data.message, 'error');
        }
    })
    .catch(error => {
        console.error('Error creating test notification:', error);
        showToast('Có lỗi xảy ra khi tạo thông báo test', 'error');
    });
}

function formatTimeAgo(dateString) {
    const date = new Date(dateString);
    const now = new Date();
    const diff = now - date;
    const oneMinute = 60000;
    const oneHour = 3600000;
    const oneDay = 86400000;
    
    if (diff < oneMinute) {
        return 'Vừa xong';
    } else if (diff < oneHour) {
        const minutes = Math.floor(diff / oneMinute);
        return `${minutes} phút trước`;
    } else if (diff < oneDay) {
        const hours = Math.floor(diff / oneHour);
        return `${hours} giờ trước`;
    } else {
        const days = Math.floor(diff / oneDay);
        return `${days} ngày trước`;
    }
}

function showToast(message, type = 'info') {
    // Simple toast notification
    const toast = document.createElement('div');
    toast.className = `alert alert-${type === 'success' ? 'success' : 'warning'} position-fixed`;
    toast.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
    toast.innerHTML = `
        <i class="fas fa-${type === 'success' ? 'check' : 'exclamation-triangle'} me-2"></i>
        ${message}
        <button type="button" class="btn-close float-end" onclick="this.parentElement.remove()"></button>
    `;
    
    document.body.appendChild(toast);
    
    // Auto remove after 3 seconds
    setTimeout(() => {
        if (toast.parentElement) {
            toast.remove();
        }
    }, 3000);
}

// Initialize notifications when document is ready
document.addEventListener('DOMContentLoaded', function() {
    // Check if user is logged in by checking for the notification elements
    const notificationBadge = document.getElementById('notification-badge');
    if (notificationBadge) {
        console.log('Initializing notifications...');
        loadNotifications();
        
        // Refresh notifications every 30 seconds
        setInterval(() => {
            loadNotifications();
        }, 30000);
        
        // Load notifications when dropdown is opened
        const notificationDropdown = document.getElementById('notificationDropdown');
        if (notificationDropdown) {
            notificationDropdown.addEventListener('click', function(e) {
                // Small delay to ensure dropdown is open
                setTimeout(() => {
                    loadNotifications();
                }, 100);
            });
        }
    }
});
