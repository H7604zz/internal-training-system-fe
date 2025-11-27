// Bài Tập JavaScript Functions

/**
 * Delete Assignment
 */
function deleteAssignment(classId, assignmentId) {
    if (!confirm('Bạn có chắc chắn muốn xóa bài tập này?')) {
        return;
    }

    const token = $('input[name="__RequestVerificationToken"]').val();

    $.ajax({
        url: `/bai-tap/${classId}/xoa/${assignmentId}`,
        type: 'POST',
        headers: {
            'RequestVerificationToken': token
        },
        success: function (response) {
            alert('Xóa bài tập thành công!');
            location.reload();
        },
        error: function (xhr) {
            const errorMsg = xhr.responseText || 'Đã xảy ra lỗi khi xóa bài tập.';
            alert(errorMsg);
        }
    });
}

/**
 * Initialize Submit Assignment Form (for Staff)
 */
function initSubmitAssignmentForm(assignmentId, classId) {
    $('#submitAssignmentForm').on('submit', function (e) {
        e.preventDefault();

        const formData = new FormData(this);
        const btnSubmit = $('#btnSubmit');

        // Validate file
        const file = $('#submissionFile')[0].files[0];
        if (!file) {
            alert('Vui lòng chọn file để nộp!');
            return;
        }

        // Check file size (100MB max)
        if (file.size > 100 * 1024 * 1024) {
            alert('Kích thước file không được vượt quá 100MB!');
            return;
        }

        // Disable button and show loading
        btnSubmit.prop('disabled', true);
        btnSubmit.html('<span class="spinner-border spinner-border-sm me-2"></span>Đang nộp...');

        $.ajax({
            url: `/bai-tap/${assignmentId}/nop-bai`,
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                alert('Nộp bài thành công!');
                location.reload();
            },
            error: function (xhr) {
                const errorMsg = xhr.responseText || 'Đã xảy ra lỗi khi nộp bài.';
                alert(errorMsg);
            },
            complete: function () {
                btnSubmit.prop('disabled', false);
                btnSubmit.html('<i class="fas fa-upload me-2"></i>Nộp Bài');
            }
        });
    });

    // File size preview and validation
    $('#submissionFile').on('change', function (e) {
        const file = e.target.files[0];
        if (file) {
            const sizeInMB = (file.size / 1024 / 1024).toFixed(2);
            
            if (file.size > 100 * 1024 * 1024) {
                alert('Kích thước file không được vượt quá 100MB!');
                $(this).val('');
                return;
            }

            console.log(`File selected: ${file.name} (${sizeInMB} MB)`);
        }
    });
}

/**
 * Grade Submission (for Mentor)
 */
function gradeSubmission(assignmentId, submissionId, score, feedback) {
    if (!score) {
        alert('Vui lòng nhập điểm!');
        return;
    }

    const scoreNum = parseFloat(score);
    if (isNaN(scoreNum) || scoreNum < 0 || scoreNum > 10) {
        alert('Điểm phải từ 0 đến 10!');
        return;
    }

    const token = $('input[name="__RequestVerificationToken"]').val();
    const data = {
        score: scoreNum,
        feedback: feedback || null
    };

    // Show loading
    const originalContent = $('.btn-primary').html();
    $('.btn-primary').prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-2"></span>Đang lưu...');

    $.ajax({
        url: `/bai-tap/${assignmentId}/bai-nop/${submissionId}/cham-diem`,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        headers: {
            'RequestVerificationToken': token
        },
        success: function (response) {
            alert('Chấm điểm thành công!');
            location.reload();
        },
        error: function (xhr) {
            const errorMsg = xhr.responseText || 'Đã xảy ra lỗi khi chấm điểm.';
            alert(errorMsg);
        },
        complete: function () {
            $('.btn-primary').prop('disabled', false).html(originalContent);
        }
    });
}

/**
 * Format file size
 */
function formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
}

/**
 * Validate date input (due date must be in future)
 */
function validateDueDate(dueDateInput) {
    const dueDate = new Date(dueDateInput);
    const now = new Date();
    
    if (dueDate <= now) {
        alert('Hạn nộp phải là thời điểm trong tương lai!');
        return false;
    }
    return true;
}

/**
 * Initialize form validation
 */
$(document).ready(function () {
    // Add Bootstrap validation classes
    $('form').on('submit', function (e) {
        if (!this.checkValidity()) {
            e.preventDefault();
            e.stopPropagation();
        }
        $(this).addClass('was-validated');
    });

    // Auto-dismiss alerts after 5 seconds
    setTimeout(function () {
        $('.alert-dismissible').fadeOut('slow');
    }, 5000);

    // Initialize tooltips
    $('[data-bs-toggle="tooltip"]').tooltip();

    // Initialize popovers
    $('[data-bs-toggle="popover"]').popover();
});

/**
 * Confirm delete with modal
 */
function confirmDelete(message, callback) {
    if (confirm(message)) {
        callback();
    }
}

/**
 * Show loading overlay
 */
function showLoading() {
    $('body').append(`
        <div id="loadingOverlay" style="
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(0,0,0,0.5);
            display: flex;
            justify-content: center;
            align-items: center;
            z-index: 9999;
        ">
            <div class="spinner-border text-light" role="status" style="width: 3rem; height: 3rem;">
                <span class="visually-hidden">Loading...</span>
            </div>
        </div>
    `);
}

/**
 * Hide loading overlay
 */
function hideLoading() {
    $('#loadingOverlay').remove();
}

/**
 * Display error message
 */
function showError(message) {
    const alertHtml = `
        <div class="alert alert-danger alert-dismissible fade show" role="alert">
            <i class="fas fa-exclamation-circle me-2"></i>${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    
    $('.container-fluid').prepend(alertHtml);
    
    // Auto-dismiss after 5 seconds
    setTimeout(function () {
        $('.alert-dismissible').fadeOut('slow', function () {
            $(this).remove();
        });
    }, 5000);
}

/**
 * Display success message
 */
function showSuccess(message) {
    const alertHtml = `
        <div class="alert alert-success alert-dismissible fade show" role="alert">
            <i class="fas fa-check-circle me-2"></i>${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    
    $('.container-fluid').prepend(alertHtml);
    
    // Auto-dismiss after 5 seconds
    setTimeout(function () {
        $('.alert-dismissible').fadeOut('slow', function () {
            $(this).remove();
        });
    }, 5000);
}

/**
 * Get file icon based on file extension
 */
function getFileIcon(filename) {
    const ext = filename.split('.').pop().toLowerCase();
    const iconMap = {
        'pdf': 'fa-file-pdf text-danger',
        'doc': 'fa-file-word text-primary',
        'docx': 'fa-file-word text-primary',
        'xls': 'fa-file-excel text-success',
        'xlsx': 'fa-file-excel text-success',
        'ppt': 'fa-file-powerpoint text-warning',
        'pptx': 'fa-file-powerpoint text-warning',
        'zip': 'fa-file-archive text-secondary',
        'rar': 'fa-file-archive text-secondary',
        'jpg': 'fa-file-image text-info',
        'jpeg': 'fa-file-image text-info',
        'png': 'fa-file-image text-info',
        'gif': 'fa-file-image text-info',
        'txt': 'fa-file-alt text-secondary',
        'default': 'fa-file text-secondary'
    };
    
    return iconMap[ext] || iconMap['default'];
}
