// Create Class JavaScript functionality
document.addEventListener('DOMContentLoaded', function() {
    let scheduleIndex = 0;
    const scheduleContainer = document.getElementById('scheduleContainer');
    const addScheduleBtn = document.getElementById('addScheduleBtn');
    const scheduleTemplate = document.getElementById('scheduleTemplate');
    const emptySchedule = document.getElementById('emptySchedule');

    // Initialize schedule index based on existing items
    const existingItems = scheduleContainer.querySelectorAll('.schedule-item');
    if (existingItems.length > 0) {
        scheduleIndex = existingItems.length;
        if (emptySchedule) {
            emptySchedule.style.display = 'none';
        }
    }

    // Add schedule item
    addScheduleBtn.addEventListener('click', function() {
        addScheduleItem();
    });

    // Remove schedule item event delegation
    scheduleContainer.addEventListener('click', function(e) {
        if (e.target.closest('.remove-schedule')) {
            e.preventDefault();
            removeScheduleItem(e.target.closest('.schedule-item'));
        }
    });

    function addScheduleItem() {
        // Hide empty message
        if (emptySchedule) {
            emptySchedule.style.display = 'none';
        }

        // Clone template
        const template = scheduleTemplate.innerHTML;
        const newItem = template.replace(/__INDEX__/g, scheduleIndex);
        
        // Create temporary container to parse HTML
        const tempDiv = document.createElement('div');
        tempDiv.innerHTML = newItem;
        const scheduleItem = tempDiv.firstElementChild;
        
        // Set data-index
        scheduleItem.setAttribute('data-index', scheduleIndex);
        
        // Add to container
        const insertPosition = scheduleContainer.querySelector('#emptySchedule') || scheduleContainer;
        if (insertPosition.id === 'emptySchedule') {
            scheduleContainer.insertBefore(scheduleItem, insertPosition);
        } else {
            scheduleContainer.appendChild(scheduleItem);
        }
        
        // Increment index
        scheduleIndex++;
        
        // Add animation
        scheduleItem.style.opacity = '0';
        scheduleItem.style.transform = 'translateY(20px)';
        
        setTimeout(() => {
            scheduleItem.style.transition = 'all 0.3s ease-out';
            scheduleItem.style.opacity = '1';
            scheduleItem.style.transform = 'translateY(0)';
        }, 10);

        // Focus on the first input
        const firstInput = scheduleItem.querySelector('input[type="date"]');
        if (firstInput) {
            firstInput.focus();
        }

        // Update form validation
        updateValidation();
    }

    function removeScheduleItem(item) {
        // Add fade out animation
        item.style.transition = 'all 0.3s ease-out';
        item.style.opacity = '0';
        item.style.transform = 'translateX(-20px)';
        
        setTimeout(() => {
            item.remove();
            
            // Show empty message if no items left
            const remainingItems = scheduleContainer.querySelectorAll('.schedule-item');
            if (remainingItems.length === 0 && emptySchedule) {
                emptySchedule.style.display = 'block';
            }
            
            // Reindex remaining items
            reindexScheduleItems();
            
            // Update form validation
            updateValidation();
        }, 300);
    }

    function reindexScheduleItems() {
        const items = scheduleContainer.querySelectorAll('.schedule-item');
        items.forEach((item, index) => {
            item.setAttribute('data-index', index);
            
            // Update all input names and IDs
            const inputs = item.querySelectorAll('input, select');
            inputs.forEach(input => {
                const name = input.getAttribute('name');
                if (name && name.includes('Schedule[')) {
                    const newName = name.replace(/Schedule\[\d+\]/, `Schedule[${index}]`);
                    input.setAttribute('name', newName);
                }
            });
        });
        
        // Update global index
        scheduleIndex = items.length;
    }

    function updateValidation() {
        // Re-run any form validation if needed
        const form = document.getElementById('createClassForm');
        if (form && typeof $.validator !== 'undefined') {
            $(form).removeData('validator').removeData('unobtrusiveValidation');
            $.validator.unobtrusive.parse(form);
        }
    }

    // Form validation
    const form = document.getElementById('createClassForm');
    if (form) {
        form.addEventListener('submit', function(e) {
            let isValid = true;
            
            // Custom validation for schedule items
            const scheduleItems = scheduleContainer.querySelectorAll('.schedule-item');
            scheduleItems.forEach(item => {
                const classDate = item.querySelector('input[name*="ClassDate"]');
                const startTime = item.querySelector('input[name*="StartTime"]');
                const endTime = item.querySelector('input[name*="EndTime"]');
                
                // Clear previous validation
                [classDate, startTime, endTime].forEach(input => {
                    if (input) {
                        input.classList.remove('is-invalid');
                        const feedback = input.parentNode.querySelector('.invalid-feedback');
                        if (feedback) feedback.remove();
                    }
                });
                
                // Validate required fields
                if (!classDate.value) {
                    showFieldError(classDate, 'Vui lòng chọn ngày học');
                    isValid = false;
                }
                
                if (!startTime.value) {
                    showFieldError(startTime, 'Vui lòng nhập giờ bắt đầu');
                    isValid = false;
                }
                
                if (!endTime.value) {
                    showFieldError(endTime, 'Vui lòng nhập giờ kết thúc');
                    isValid = false;
                }
                
                // Validate class date is not in the past
                if (classDate.value) {
                    const selectedDate = new Date(classDate.value);
                    const today = new Date();
                    today.setHours(0, 0, 0, 0);
                    
                    if (selectedDate < today) {
                        showFieldError(classDate, 'Ngày học không thể là quá khứ');
                        isValid = false;
                    }
                }
                
                // Validate time logic
                if (startTime.value && endTime.value) {
                    const start = new Date(`1970-01-01T${startTime.value}:00`);
                    const end = new Date(`1970-01-01T${endTime.value}:00`);
                    
                    if (end <= start) {
                        showFieldError(endTime, 'Giờ kết thúc phải sau giờ bắt đầu');
                        isValid = false;
                    }
                }
            });
            
            // Check for duplicate dates
            const selectedDates = Array.from(scheduleItems).map(item => 
                item.querySelector('input[name*="ClassDate"]').value
            ).filter(date => date);
            
            const uniqueDates = new Set(selectedDates);
            if (selectedDates.length !== uniqueDates.size) {
                alert('Không thể chọn trùng ngày trong lịch học!');
                isValid = false;
            }
            
            if (!isValid) {
                e.preventDefault();
                
                // Scroll to first error
                const firstError = form.querySelector('.is-invalid');
                if (firstError) {
                    firstError.scrollIntoView({ behavior: 'smooth', block: 'center' });
                    firstError.focus();
                }
            } else {
                // Show loading state
                const submitBtn = form.querySelector('button[type="submit"]');
                if (submitBtn) {
                    submitBtn.disabled = true;
                    submitBtn.classList.add('btn-loading');
                    submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Đang tạo...';
                }
            }
        });
    }

    function showFieldError(field, message) {
        field.classList.add('is-invalid');
        
        // Remove existing error message
        const existingFeedback = field.parentNode.querySelector('.invalid-feedback');
        if (existingFeedback) {
            existingFeedback.remove();
        }
        
        // Add new error message
        const feedback = document.createElement('div');
        feedback.className = 'invalid-feedback';
        feedback.textContent = message;
        field.parentNode.appendChild(feedback);
    }

    // Date validation
    const startDateInput = document.querySelector('input[name="StartDate"]');
    const endDateInput = document.querySelector('input[name="EndDate"]');
    
    if (startDateInput && endDateInput) {
        function validateDates() {
            const startDate = new Date(startDateInput.value);
            const endDate = new Date(endDateInput.value);
            const today = new Date();
            today.setHours(0, 0, 0, 0);
            
            // Clear previous validation
            [startDateInput, endDateInput].forEach(input => {
                input.classList.remove('is-invalid');
                const feedback = input.parentNode.querySelector('.invalid-feedback');
                if (feedback) feedback.remove();
            });
            
            // Start date validation
            if (startDate < today) {
                showFieldError(startDateInput, 'Ngày bắt đầu không thể là quá khứ');
                return false;
            }
            
            // End date validation
            if (endDate <= startDate) {
                showFieldError(endDateInput, 'Ngày kết thúc phải sau ngày bắt đầu');
                return false;
            }
            
            return true;
        }
        
        startDateInput.addEventListener('change', validateDates);
        endDateInput.addEventListener('change', validateDates);
    }

    // Capacity validation
    const capacityInput = document.querySelector('input[name="Capacity"]');
    if (capacityInput) {
        capacityInput.addEventListener('input', function() {
            const value = parseInt(this.value);
            
            this.classList.remove('is-invalid');
            const feedback = this.parentNode.querySelector('.invalid-feedback');
            if (feedback) feedback.remove();
            
            if (value < 1 || value > 100) {
                showFieldError(this, 'Sức chứa phải từ 1 đến 100 học viên');
            }
        });
    }

    // Auto-resize textarea
    const textareas = document.querySelectorAll('textarea');
    textareas.forEach(textarea => {
        textarea.addEventListener('input', function() {
            this.style.height = 'auto';
            this.style.height = (this.scrollHeight) + 'px';
        });
    });

    // Initialize tooltips if Bootstrap is available
    if (typeof bootstrap !== 'undefined') {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }

    // Course and Mentor change handlers
    const courseSelect = document.querySelector('select[name="CourseId"]');
    const mentorSelect = document.querySelector('select[name="MentorId"]');
    
    if (courseSelect) {
        courseSelect.addEventListener('change', function() {
            // You can add course-specific logic here
            // For example, filtering mentors based on course
            console.log('Course selected:', this.value);
        });
    }
    
    if (mentorSelect) {
        mentorSelect.addEventListener('change', function() {
            // You can add mentor-specific logic here
            console.log('Mentor selected:', this.value);
        });
    }
});