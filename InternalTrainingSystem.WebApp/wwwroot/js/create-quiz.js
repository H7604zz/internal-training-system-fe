// Create Quiz JavaScript Functions
$(document).ready(function() {
    initializeCreateQuiz();
    setupEventHandlers();
    updatePreview();
});

function initializeCreateQuiz() {
    // Set default dates
    const now = new Date();
    const nextWeek = new Date(now.getTime() + 7 * 24 * 60 * 60 * 1000);
    
    if (!$('#StartDate').val()) {
        $('#StartDate').val(formatDateTimeLocal(now));
    }
    if (!$('#EndDate').val()) {
        $('#EndDate').val(formatDateTimeLocal(nextWeek));
    }
}

function setupEventHandlers() {
    // Chapter selection change
    $('#chapterSelect').change(function() {
        const chapterId = $(this).val();
        const selectedOption = $(this).find('option:selected');
        
        if (chapterId) {
            // Show chapter description
            const description = selectedOption.data('description');
            $('#chapterDescription').html(`<i class="fas fa-info-circle text-info"></i> ${description}`);
            
            // Load sessions for selected chapter
            loadSessionsByChapter(chapterId);
        } else {
            $('#chapterDescription').html('');
            clearSessions();
        }
        
        updatePreview();
    });

    // Session selection change
    $('#sessionSelect').change(function() {
        const sessionId = $(this).val();
        const selectedOption = $(this).find('option:selected');
        
        if (sessionId) {
            const questionCount = selectedOption.data('questions');
            $('#sessionInfo').html(`<i class="fas fa-database text-success"></i> Có sẵn ${questionCount} câu hỏi`);
            $('#maxQuestions').text(questionCount);
            
            // Update max value for question count input
            $('#QuestionCount').attr('max', questionCount);
            
            // Adjust question count if it exceeds available questions
            const currentQuestionCount = parseInt($('#QuestionCount').val());
            if (currentQuestionCount > questionCount) {
                $('#QuestionCount').val(questionCount);
            }
        } else {
            $('#sessionInfo').html('');
            $('#maxQuestions').text('0');
        }
        
        updatePreview();
    });

    // Form inputs change
    $('#Title, #QuestionCount, #Duration, #StartDate, #EndDate').on('input change', function() {
        updatePreview();
    });

    // Form validation
    $('#createQuizForm').submit(function(e) {
        if (!validateForm()) {
            e.preventDefault();
            return false;
        }
    });

    // Real-time validation
    $('#QuestionCount').on('input', function() {
        validateQuestionCount();
    });

    $('#Duration').on('input', function() {
        validateDuration();
    });

    $('#EndDate').on('change', function() {
        validateDates();
    });

    $('#StartDate').on('change', function() {
        validateDates();
    });
}

function loadSessionsByChapter(chapterId) {
    // Show loading state
    $('#sessionSelect').addClass('loading').prop('disabled', true);
    
    $.ajax({
        url: '/Quiz/GetSessionsByChapter',
        type: 'GET',
        data: { chapterId: chapterId },
        success: function(sessions) {
            populateSessions(sessions);
        },
        error: function() {
            showErrorMessage('Không thể tải danh sách session. Vui lòng thử lại.');
        },
        complete: function() {
            $('#sessionSelect').removeClass('loading').prop('disabled', false);
        }
    });
}

function populateSessions(sessions) {
    const sessionSelect = $('#sessionSelect');
    sessionSelect.empty();
    sessionSelect.append('<option value="">-- Chọn Session --</option>');
    
    sessions.forEach(function(session) {
        sessionSelect.append(`
            <option value="${session.id}" data-questions="${session.questionCount}">
                ${session.name} (${session.questionCount} câu hỏi)
            </option>
        `);
    });
    
    sessionSelect.prop('disabled', false);
}

function clearSessions() {
    $('#sessionSelect').empty()
        .append('<option value="">-- Chọn Session --</option>')
        .prop('disabled', true);
    $('#sessionInfo').html('');
    $('#maxQuestions').text('0');
}

function updatePreview() {
    // Update title
    const title = $('#Title').val() || 'Chưa nhập';
    $('#previewTitle').text(title);
    
    // Update chapter
    const chapterText = $('#chapterSelect option:selected').text() || 'Chưa chọn';
    $('#previewChapter').text(chapterText);
    
    // Update session
    const sessionText = $('#sessionSelect option:selected').text() || 'Chưa chọn';
    $('#previewSession').text(sessionText);
    
    // Update question count
    const questionCount = $('#QuestionCount').val() || '0';
    $('#previewQuestions').text(questionCount);
    
    // Update duration
    const duration = $('#Duration').val() || '0';
    $('#previewDuration').text(duration);
    
    // Update start date
    const startDate = $('#StartDate').val();
    if (startDate) {
        const formattedStartDate = formatDisplayDate(new Date(startDate));
        $('#previewStartDate').text(formattedStartDate);
    }
}

function validateForm() {
    let isValid = true;
    
    // Validate required fields
    if (!$('#Title').val().trim()) {
        showFieldError('#Title', 'Vui lòng nhập tiêu đề bài kiểm tra');
        isValid = false;
    }
    
    if (!$('#SelectedChapterId').val()) {
        showFieldError('#chapterSelect', 'Vui lòng chọn chương');
        isValid = false;
    }
    
    if (!$('#SelectedSessionId').val()) {
        showFieldError('#sessionSelect', 'Vui lòng chọn session');
        isValid = false;
    }
    
    // Validate question count
    if (!validateQuestionCount()) {
        isValid = false;
    }
    
    // Validate duration
    if (!validateDuration()) {
        isValid = false;
    }
    
    // Validate dates
    if (!validateDates()) {
        isValid = false;
    }
    
    return isValid;
}

function validateQuestionCount() {
    const questionCount = parseInt($('#QuestionCount').val());
    const maxQuestions = parseInt($('#maxQuestions').text()) || 0;
    
    if (!questionCount || questionCount < 1) {
        showFieldError('#QuestionCount', 'Số lượng câu hỏi phải lớn hơn 0');
        return false;
    }
    
    if (maxQuestions > 0 && questionCount > maxQuestions) {
        showFieldError('#QuestionCount', `Số lượng câu hỏi không được vượt quá ${maxQuestions}`);
        return false;
    }
    
    clearFieldError('#QuestionCount');
    return true;
}

function validateDuration() {
    const duration = parseInt($('#Duration').val());
    
    if (!duration || duration < 5) {
        showFieldError('#Duration', 'Thời gian làm bài phải ít nhất 5 phút');
        return false;
    }
    
    if (duration > 180) {
        showFieldError('#Duration', 'Thời gian làm bài không được quá 180 phút');
        return false;
    }
    
    clearFieldError('#Duration');
    return true;
}

function validateDates() {
    const startDate = new Date($('#StartDate').val());
    const endDate = new Date($('#EndDate').val());
    const now = new Date();
    
    if (startDate < now) {
        showFieldError('#StartDate', 'Ngày bắt đầu không được trong quá khứ');
        return false;
    }
    
    if (endDate <= startDate) {
        showFieldError('#EndDate', 'Ngày kết thúc phải sau ngày bắt đầu');
        return false;
    }
    
    clearFieldError('#StartDate');
    clearFieldError('#EndDate');
    return true;
}

function showFieldError(fieldSelector, message) {
    const field = $(fieldSelector);
    field.addClass('is-invalid');
    
    // Remove existing error message
    field.next('.text-danger').remove();
    
    // Add new error message
    field.after(`<span class="text-danger">${message}</span>`);
}

function clearFieldError(fieldSelector) {
    const field = $(fieldSelector);
    field.removeClass('is-invalid');
    field.next('.text-danger').remove();
}

function showErrorMessage(message) {
    // You can implement a toast notification here
    alert(message);
}

function formatDateTimeLocal(date) {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    
    return `${year}-${month}-${day}T${hours}:${minutes}`;
}

function formatDisplayDate(date) {
    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    
    return `${day}/${month}/${year} ${hours}:${minutes}`;
}

// Auto-save functionality (optional)
function autoSave() {
    const formData = {
        title: $('#Title').val(),
        description: $('#Description').val(),
        chapterId: $('#SelectedChapterId').val(),
        sessionId: $('#SelectedSessionId').val(),
        questionCount: $('#QuestionCount').val(),
        duration: $('#Duration').val(),
        startDate: $('#StartDate').val(),
        endDate: $('#EndDate').val(),
        isRandom: $('#IsRandomQuestions').is(':checked')
    };
    
    localStorage.setItem('createQuizDraft', JSON.stringify(formData));
}

function loadDraft() {
    const draft = localStorage.getItem('createQuizDraft');
    if (draft) {
        const data = JSON.parse(draft);
        
        $('#Title').val(data.title);
        $('#Description').val(data.description);
        $('#QuestionCount').val(data.questionCount);
        $('#Duration').val(data.duration);
        $('#StartDate').val(data.startDate);
        $('#EndDate').val(data.endDate);
        $('#IsRandomQuestions').prop('checked', data.isRandom);
        
        if (data.chapterId) {
            $('#SelectedChapterId').val(data.chapterId).trigger('change');
            
            setTimeout(() => {
                if (data.sessionId) {
                    $('#SelectedSessionId').val(data.sessionId).trigger('change');
                }
            }, 500);
        }
    }
}

// Auto-save every 30 seconds
setInterval(autoSave, 30000);

// Clear draft on successful submission
$('#createQuizForm').on('submit', function() {
    localStorage.removeItem('createQuizDraft');
});