document.addEventListener('DOMContentLoaded', function() {
    let selectedSchedules = [];
    
    function generateCalendar() {
        const weeksCount = parseInt(document.getElementById('weeksCount').value) || 12;
        const daysPerWeek = parseInt(document.getElementById('daysPerWeek').value) || 2;
        
        selectedSchedules = [];
        const startDate = new Date();
        startDate.setDate(startDate.getDate() + 1);
        
        let calendarHtml = generateWeekTemplate(daysPerWeek) + generateWeeksCalendar(weeksCount, daysPerWeek, startDate);
        
        const calendarContainer = document.getElementById('calendarContainer');
        if (calendarContainer) {
            calendarContainer.innerHTML = calendarHtml;
        }
        
        initializeCalendarEvents();
    }
    
    function generateWeekTemplate(daysPerWeek) {
        const daysOfWeek = ['Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7', 'Chủ nhật'];
        let templateHtml = '<div class="week-template mb-4 p-3 border rounded bg-light">';
        templateHtml += '<h6 class="text-primary mb-3"><i class="fas fa-clock me-2"></i>Mẫu lịch tuần</h6>';
        templateHtml += '<div class="row template-days">';
        
        for (let d = 0; d < 7; d++) {
            const isSelected = d < daysPerWeek;
            templateHtml += '<div class="col-md-1 mb-2">';
            templateHtml += '<div class="day-template ' + (isSelected ? 'selected' : '') + '" data-day="' + d + '">';
            templateHtml += '<div class="day-header text-center py-2 ' + (isSelected ? 'bg-primary text-white' : 'bg-secondary text-white') + '">';
            templateHtml += '<small>' + daysOfWeek[d] + '</small>';
            templateHtml += '</div>';
            templateHtml += '<div class="day-content p-2">';
            
            if (isSelected) {
                templateHtml += '<div class="time-inputs">';
                templateHtml += '<input type="time" class="form-control form-control-sm mb-1 start-time" value="08:00">';
                templateHtml += '<input type="time" class="form-control form-control-sm end-time" value="17:00">';
                templateHtml += '<input type="text" class="form-control form-control-sm mt-1 room-input" placeholder="Phòng" value="A101">';
                templateHtml += '</div>';
            } else {
                templateHtml += '<div class="text-center text-muted"><small>Không học</small></div>';
            }
            
            templateHtml += '</div></div></div>';
        }
        
        templateHtml += '</div>';
        templateHtml += '<div class="text-center mt-3">';
        templateHtml += '<button type="button" class="btn btn-sm btn-outline-primary" id="applyToAllWeeks">';
        templateHtml += '<i class="fas fa-copy me-2"></i>Áp dụng cho tất cả các tuần';
        templateHtml += '</button>';
        templateHtml += '</div></div>';
        
        return templateHtml;
    }
    
    function generateWeeksCalendar(weeksCount, daysPerWeek, startDate) {
        const daysOfWeek = ['Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7', 'Chủ nhật'];
        let weeksHtml = '<div class="weeks-container">';
        
        for (let week = 1; week <= weeksCount; week++) {
            weeksHtml += '<div class="week-item mb-3 p-3 border rounded" data-week="' + week + '">';
            weeksHtml += '<h6 class="text-success mb-3"><i class="fas fa-calendar-week me-2"></i>Tuần ' + week + '</h6>';
            weeksHtml += '<div class="row week-days" data-week="' + week + '">';
            
            for (let d = 0; d < 7; d++) {
                const currentDate = new Date(startDate);
                currentDate.setDate(startDate.getDate() + (week - 1) * 7 + d);
                const dateStr = currentDate.toLocaleDateString('vi-VN');
                const isSelected = d < daysPerWeek;
                
                weeksHtml += '<div class="col-md-1 mb-2">';
                weeksHtml += '<div class="day-item ' + (isSelected ? 'selected' : '') + '" data-week="' + week + '" data-day="' + d + '" data-date="' + currentDate.toISOString().split('T')[0] + '">';
                weeksHtml += '<div class="day-header text-center py-1 ' + (isSelected ? 'bg-success text-white' : 'bg-light') + '">';
                weeksHtml += '<small>' + daysOfWeek[d] + '</small><br><small>' + dateStr + '</small>';
                weeksHtml += '</div>';
                weeksHtml += '<div class="day-content p-2">';
                
                if (isSelected) {
                    weeksHtml += '<div class="schedule-info">';
                    weeksHtml += '<input type="time" class="form-control form-control-sm mb-1 start-time" value="08:00">';
                    weeksHtml += '<input type="time" class="form-control form-control-sm end-time" value="17:00">';
                    weeksHtml += '<input type="text" class="form-control form-control-sm mt-1 room-input" placeholder="Phòng" value="A101">';
                    weeksHtml += '</div>';
                } else {
                    weeksHtml += '<div class="text-center text-muted"><small>Nghỉ</small></div>';
                }
                
                weeksHtml += '</div></div></div>';
            }
            
            weeksHtml += '</div></div>';
        }
        
        weeksHtml += '</div>';
        return weeksHtml;
    }
    
    function initializeCalendarEvents() {
        const applyBtn = document.getElementById('applyToAllWeeks');
        if (applyBtn) {
            applyBtn.addEventListener('click', applyTemplateToAllWeeks);
        }
        
        const dayTemplates = document.querySelectorAll('.day-template');
        dayTemplates.forEach(function(template) {
            template.addEventListener('click', toggleDayTemplate);
        });
    }
    
    function applyTemplateToAllWeeks() {
        const templateData = [];
        const selectedTemplates = document.querySelectorAll('.day-template.selected');
        
        selectedTemplates.forEach(function(template) {
            const dayIndex = template.getAttribute('data-day');
            const startTime = template.querySelector('.start-time');
            const endTime = template.querySelector('.end-time');
            const room = template.querySelector('.room-input');
            
            templateData.push({
                day: dayIndex,
                startTime: startTime ? startTime.value : '08:00',
                endTime: endTime ? endTime.value : '17:00',
                room: room ? room.value : 'A101'
            });
        });
        
        const weekItems = document.querySelectorAll('.week-item');
        weekItems.forEach(function(weekItem) {
            const weekNumber = weekItem.getAttribute('data-week');
            
            templateData.forEach(function(template) {
                const dayElement = document.querySelector('.day-item[data-week="' + weekNumber + '"][data-day="' + template.day + '"]');
                if (dayElement) {
                    const startTimeInput = dayElement.querySelector('.start-time');
                    const endTimeInput = dayElement.querySelector('.end-time');
                    const roomInput = dayElement.querySelector('.room-input');
                    
                    if (startTimeInput) startTimeInput.value = template.startTime;
                    if (endTimeInput) endTimeInput.value = template.endTime;
                    if (roomInput) roomInput.value = template.room;
                }
            });
        });
        
        alert('Đã áp dụng mẫu lịch cho tất cả các tuần!');
    }
    
    function toggleDayTemplate() {
        this.classList.toggle('selected');
        const isSelected = this.classList.contains('selected');
        const dayHeader = this.querySelector('.day-header');
        const dayContent = this.querySelector('.day-content');
        
        if (isSelected) {
            dayHeader.classList.remove('bg-secondary');
            dayHeader.classList.add('bg-primary');
            dayContent.innerHTML = '<div class="time-inputs">' +
                '<input type="time" class="form-control form-control-sm mb-1 start-time" value="08:00">' +
                '<input type="time" class="form-control form-control-sm end-time" value="17:00">' +
                '<input type="text" class="form-control form-control-sm mt-1 room-input" placeholder="Phòng" value="A101">' +
                '</div>';
        } else {
            dayHeader.classList.remove('bg-primary');
            dayHeader.classList.add('bg-secondary');
            dayContent.innerHTML = '<div class="text-center text-muted"><small>Không học</small></div>';
        }
    }
    
    function confirmScheduleSelection() {
        selectedSchedules = [];
        const selectedDays = document.querySelectorAll('.day-item.selected');
        
        selectedDays.forEach(function(dayItem) {
            const date = dayItem.getAttribute('data-date');
            const startTimeInput = dayItem.querySelector('.start-time');
            const endTimeInput = dayItem.querySelector('.end-time');
            const roomInput = dayItem.querySelector('.room-input');
            
            const startTime = startTimeInput ? startTimeInput.value : '';
            const endTime = endTimeInput ? endTimeInput.value : '';
            const room = roomInput ? roomInput.value : '';
            
            if (date && startTime && endTime) {
                selectedSchedules.push({
                    date: date,
                    startTime: startTime,
                    endTime: endTime,
                    room: room || 'TBA'
                });
            }
        });
        
        if (selectedSchedules.length === 0) {
            alert('Vui lòng chọn ít nhất một ngày học!');
            return;
        }
        
        alert('Đã chọn ' + selectedSchedules.length + ' buổi học thành công!');
        localStorage.setItem('selectedSchedules', JSON.stringify(selectedSchedules));
    }
    
    // Initialize calendar
    const generateBtn = document.getElementById('generateCalendarBtn');
    const confirmBtn = document.getElementById('confirmScheduleBtn');
    
    if (generateBtn) {
        generateBtn.addEventListener('click', generateCalendar);
    }
    
    if (confirmBtn) {
        confirmBtn.addEventListener('click', confirmScheduleSelection);
    }
    
    // Generate initial calendar
    generateCalendar();
});