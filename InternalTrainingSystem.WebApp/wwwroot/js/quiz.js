// Quiz JavaScript Functions
let currentQuestionIndex = 0;
let timeRemaining = 60 * 60; // 60 minutes in seconds
let timerInterval;
let answers = {};

// Initialize quiz when page loads
$(document).ready(function() {
    initializeQuiz();
    startTimer();
    setupEventHandlers();
});

function initializeQuiz() {
    // Show first question
    showQuestion(0);
    
    // Update progress
    updateQuestionProgress();
}

function startTimer() {
    timerInterval = setInterval(function() {
        timeRemaining--;
        updateTimerDisplay();
        
        if (timeRemaining <= 0) {
            // Auto submit when time is up
            autoSubmitQuiz();
        }
    }, 1000);
}

function updateTimerDisplay() {
    const minutes = Math.floor(timeRemaining / 60);
    const seconds = timeRemaining % 60;
    
    $('#minutes').text(minutes.toString().padStart(2, '0'));
    $('#seconds').text(seconds.toString().padStart(2, '0'));
    
    // Update progress bar
    const totalTime = 60 * 60; // 60 minutes
    const progressPercentage = (timeRemaining / totalTime) * 100;
    $('#timeProgress').css('width', progressPercentage + '%');
    
    // Change timer color based on remaining time
    const timerElement = $('#timer');
    if (timeRemaining <= 300) { // 5 minutes left
        timerElement.addClass('danger').removeClass('warning');
    } else if (timeRemaining <= 600) { // 10 minutes left
        timerElement.addClass('warning').removeClass('danger');
    }
}

function setupEventHandlers() {
    // Question item click handlers
    $('.question-item').click(function() {
        const questionId = $(this).data('question-id');
        const questionIndex = findQuestionIndex(questionId);
        showQuestion(questionIndex);
    });
    
    // Radio button change handlers
    $('input[type="radio"]').change(function() {
        const questionId = parseInt($(this).attr('name').match(/\d+/)[0]);
        const answerValue = parseInt($(this).val());
        answers[questionId] = answerValue;
        
        markAnswered(questionId);
        updateQuestionProgress();
    });
}

function findQuestionIndex(questionId) {
    const questionCards = $('.question-card');
    for (let i = 0; i < questionCards.length; i++) {
        if ($(questionCards[i]).data('question-id') == questionId) {
            return i;
        }
    }
    return 0;
}

function showQuestion(index) {
    // Hide all questions
    $('.question-card').removeClass('active');
    $('.question-item').removeClass('active');
    
    // Show selected question
    $('.question-card').eq(index).addClass('active');
    $('.question-item').eq(index).addClass('active');
    
    currentQuestionIndex = index;
    
    // Scroll to top of question content
    $('.right-panel').scrollTop(0);
}

function nextQuestion() {
    if (currentQuestionIndex < $('.question-card').length - 1) {
        showQuestion(currentQuestionIndex + 1);
    }
}

function previousQuestion() {
    if (currentQuestionIndex > 0) {
        showQuestion(currentQuestionIndex - 1);
    }
}

function markAnswered(questionId) {
    const questionItem = $(`.question-item[data-question-id="${questionId}"]`);
    const statusIcon = questionItem.find('.question-status i');
    
    statusIcon.removeClass('text-secondary').addClass('text-success');
    statusIcon.removeClass('fa-circle').addClass('fa-check-circle');
}

function updateQuestionProgress() {
    const totalQuestions = $('.question-card').length;
    const answeredQuestions = Object.keys(answers).length;
    
    // Update summary in submit modal if needed
    $('#submitSummary').html(`
        <div class="alert alert-info">
            <i class="fas fa-info-circle"></i>
            Bạn đã trả lời <strong>${answeredQuestions}</strong> / <strong>${totalQuestions}</strong> câu hỏi.
        </div>
    `);
}

function submitQuiz() {
    updateQuestionProgress();
    $('#submitModal').modal('show');
}

function confirmSubmit() {
    clearInterval(timerInterval);
    
    // Populate answers in form
    for (const [questionId, answer] of Object.entries(answers)) {
        $(`input[name="Answers[${questionId}]"][value="${answer}"]`).prop('checked', true);
    }
    
    $('#quizForm').submit();
}

function autoSubmitQuiz() {
    clearInterval(timerInterval);
    
    // Show time up message
    alert('Hết thời gian! Bài thi sẽ được nộp tự động.');
    
    // Auto submit
    confirmSubmit();
}

// Prevent page refresh during quiz
window.addEventListener('beforeunload', function(e) {
    if (timeRemaining > 0) {
        const confirmationMessage = 'Bạn có chắc chắn muốn rời khỏi trang? Bài thi của bạn sẽ không được lưu.';
        e.returnValue = confirmationMessage;
        return confirmationMessage;
    }
});

// Keyboard shortcuts
$(document).keydown(function(e) {
    // Press right arrow or space to go to next question
    if (e.which === 39 || e.which === 32) {
        e.preventDefault();
        nextQuestion();
    }
    // Press left arrow to go to previous question
    else if (e.which === 37) {
        e.preventDefault();
        previousQuestion();
    }
    // Press number keys (1-9) to select option
    else if (e.which >= 49 && e.which <= 57) {
        const optionIndex = e.which - 49;
        const currentQuestionCard = $('.question-card.active');
        const radioButtons = currentQuestionCard.find('input[type="radio"]');
        
        if (optionIndex < radioButtons.length) {
            $(radioButtons[optionIndex]).prop('checked', true).trigger('change');
        }
    }
});

// Auto-save answers to localStorage (optional)
function saveProgress() {
    const progress = {
        answers: answers,
        currentQuestion: currentQuestionIndex,
        timeRemaining: timeRemaining
    };
    localStorage.setItem('quizProgress', JSON.stringify(progress));
}

function loadProgress() {
    const savedProgress = localStorage.getItem('quizProgress');
    if (savedProgress) {
        const progress = JSON.parse(savedProgress);
        answers = progress.answers || {};
        currentQuestionIndex = progress.currentQuestion || 0;
        // Note: Don't restore time remaining to prevent cheating
        
        // Restore answered questions visual state
        for (const questionId of Object.keys(answers)) {
            markAnswered(parseInt(questionId));
        }
        
        // Restore radio button states
        for (const [questionId, answer] of Object.entries(answers)) {
            $(`input[name="Answers[${questionId}]"][value="${answer}"]`).prop('checked', true);
        }
    }
}

// Save progress periodically
setInterval(saveProgress, 10000); // Save every 10 seconds