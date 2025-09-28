// Internal Training System - Custom JavaScript

// Initialize when document ready
$(document).ready(function() {
    // Add fade-in animation to cards
    $('.card').addClass('fade-in');
    
    // Initialize tooltips
    $('[data-bs-toggle="tooltip"]').tooltip();
    
    // Auto-hide alerts after 5 seconds
    $('.alert').delay(5000).fadeOut();
    
    // Form validation feedback
    $('form').on('submit', function() {
        $(this).addClass('was-validated');
    });
    
    // Loading state for buttons
    $('.btn').on('click', function() {
        var $btn = $(this);
        if ($btn.attr('type') === 'submit') {
            $btn.addClass('loading');
            setTimeout(function() {
                $btn.removeClass('loading');
            }, 2000);
        }
    });
    
    // Search functionality with debounce
    let searchTimeout;
    $('input[name="searchTerm"]').on('input', function() {
        clearTimeout(searchTimeout);
        const form = $(this).closest('form');
        searchTimeout = setTimeout(function() {
            // Auto-submit search form after 500ms delay
            // form.submit();
        }, 500);
    });
    
    // Dynamic progress bars
    $('.progress-bar').each(function() {
        const width = $(this).attr('style').match(/width:\s*(\d+)%/);
        if (width) {
            $(this).animate({
                width: width[1] + '%'
            }, 1000);
        }
    });
    
    // Smooth scrolling for anchor links
    $('a[href^="#"]').on('click', function(event) {
        const target = $(this.getAttribute('href'));
        if (target.length) {
            event.preventDefault();
            $('html, body').stop().animate({
                scrollTop: target.offset().top - 100
            }, 1000);
        }
    });
});

// Utility functions
const TrainingSystem = {
    // Show success message
    showSuccess: function(message) {
        this.showAlert(message, 'success');
    },
    
    // Show error message
    showError: function(message) {
        this.showAlert(message, 'danger');
    },
    
    // Show info message
    showInfo: function(message) {
        this.showAlert(message, 'info');
    },
    
    // Show alert with auto-hide
    showAlert: function(message, type) {
        const alertHtml = `
            <div class="alert alert-${type} alert-dismissible fade show" role="alert">
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;
        
        // Add to top of main content
        $('main').prepend(alertHtml);
        
        // Auto-hide after 5 seconds
        setTimeout(function() {
            $('.alert').first().fadeOut();
        }, 5000);
    },
    
    // Format date to Vietnamese format
    formatDate: function(dateString) {
        const date = new Date(dateString);
        return date.toLocaleDateString('vi-VN');
    },
    
    // Format number with thousand separators
    formatNumber: function(number) {
        return number.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ".");
    },
    
    // Confirm deletion
    confirmDelete: function(message) {
        return confirm(message || 'Bạn có chắc chắn muốn xóa?');
    },
    
    // Loading overlay
    showLoading: function() {
        if ($('#loading-overlay').length === 0) {
            $('body').append(`
                <div id="loading-overlay" class="d-flex justify-content-center align-items-center position-fixed w-100 h-100" style="top:0;left:0;background:rgba(0,0,0,0.5);z-index:9999;">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                </div>
            `);
        }
    },
    
    hideLoading: function() {
        $('#loading-overlay').remove();
    }
};

// Global error handler for AJAX requests
$(document).ajaxError(function(event, xhr, settings, thrownError) {
    TrainingSystem.hideLoading();
    TrainingSystem.showError('Có lỗi xảy ra khi tải dữ liệu. Vui lòng thử lại.');
});

// Global success handler for AJAX requests
$(document).ajaxSuccess(function(event, xhr, settings) {
    TrainingSystem.hideLoading();
});

// Show loading for AJAX requests
$(document).ajaxStart(function() {
    TrainingSystem.showLoading();
});

// Add to global scope
window.TrainingSystem = TrainingSystem;
