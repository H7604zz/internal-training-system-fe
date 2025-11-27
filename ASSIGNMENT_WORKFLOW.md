# LUỒNG HOẠT ĐỘNG - QUẢN LÝ BÀI TẬP (ASSIGNMENT)

## 📋 MỤC LỤC
1. [Tổng Quan](#tổng-quan)
2. [Luồng Của Mentor](#luồng-của-mentor)
3. [Luồng Của Staff](#luồng-của-staff)
4. [Sơ Đồ Luồng](#sơ-đồ-luồng)
5. [API Endpoints](#api-endpoints)

---

## 🎯 TỔNG QUAN

Hệ thống quản lý bài tập cho phép:
- **Mentor**: Tạo, sửa, xóa bài tập và chấm điểm bài nộp
- **Staff**: Xem bài tập và nộp bài làm

---

## 👨‍🏫 LUỒNG CỦA MENTOR

### 1️⃣ XEM DANH SÁCH BÀI TẬP

**URL**: `/bai-tap/{classId}`

**Các Bước:**
```
1. Mentor truy cập vào lớp học từ "Lớp Học Của Tôi"
2. Click vào "Chi Tiết Lớp Học"
3. Click vào "Bài Tập" hoặc URL trực tiếp /bai-tap/{classId}
4. Hệ thống hiển thị danh sách tất cả bài tập của lớp
   - Tiêu đề bài tập
   - Mô tả ngắn
   - Hạn nộp
   - Trạng thái (Còn hạn/Quá hạn)
   - File đính kèm (nếu có)
5. Mentor có thể:
   - Xem chi tiết bài tập
   - Tạo bài tập mới
   - Sửa bài tập
   - Xóa bài tập
   - Xem danh sách bài nộp
```

**Controller Action**: `BaiTapController.Index(classId)`

**API Call**: `GET /api/assignment/{classId}`

---

### 2️⃣ TẠO BÀI TẬP MỚI

**URL**: `/bai-tap/{classId}/tao-moi`

**Các Bước:**
```
1. Từ danh sách bài tập, click "Tạo Bài Tập Mới"
2. Hệ thống hiển thị form với các trường:
   ├─ Tiêu đề (*) - Tối đa 200 ký tự
   ├─ Mô tả - Nội dung chi tiết bài tập
   ├─ Hạn nộp (*) - DateTime picker (phải > hiện tại)
   └─ File đính kèm - Upload tài liệu (tối đa 100MB)
3. Mentor điền thông tin và upload file (nếu có)
4. Click "Tạo Bài Tập"
5. Hệ thống validate:
   ├─ Tiêu đề không rỗng
   ├─ Hạn nộp phải là thời điểm tương lai
   └─ File không vượt quá 100MB
6. Nếu hợp lệ:
   ├─ Upload file lên server
   ├─ Lưu thông tin bài tập vào DB
   └─ Redirect về danh sách bài tập với thông báo thành công
7. Nếu lỗi: Hiển thị thông báo lỗi
```

**Controller Action**: 
- GET: `BaiTapController.TaoMoi(classId)`
- POST: `BaiTapController.TaoMoi(classId, CreateAssignmentForm)`

**API Call**: `POST /api/assignment/{classId}`

**Form Data**:
```
- ClassId: int
- Title: string
- Description: string (optional)
- DueDate: DateTime
- AttachmentFile: IFormFile (optional)
```

---

### 3️⃣ SỬA BÀI TẬP

**URL**: `/bai-tap/{classId}/sua/{assignmentId}`

**Các Bước:**
```
1. Từ danh sách bài tập, click menu dropdown → "Sửa"
2. Hệ thống load thông tin hiện tại của bài tập:
   ├─ Tiêu đề
   ├─ Mô tả
   ├─ Hạn nộp
   └─ File đính kèm hiện tại (nếu có)
3. Mentor chỉnh sửa thông tin:
   ├─ Có thể giữ nguyên file cũ
   ├─ Có thể xóa file cũ (checkbox "Xóa file này")
   └─ Có thể upload file mới (sẽ thay thế file cũ)
4. Click "Cập Nhật"
5. Hệ thống validate và lưu thay đổi
6. Redirect về chi tiết bài tập với thông báo thành công
```

**Controller Action**:
- GET: `BaiTapController.Sua(classId, assignmentId)`
- POST: `BaiTapController.Sua(classId, assignmentId, UpdateAssignmentForm)`

**API Call**: `PUT /api/assignment/{classId}/{assignmentId}`

**Form Data**:
```
- Title: string
- Description: string (optional)
- DueDate: DateTime
- RemoveAttachment: bool
- AttachmentFile: IFormFile (optional)
```

---

### 4️⃣ XÓA BÀI TẬP

**URL**: `/bai-tap/{classId}/xoa/{assignmentId}` (AJAX)

**Các Bước:**
```
1. Từ danh sách bài tập, click menu dropdown → "Xóa"
2. Hệ thống hiển thị confirm dialog:
   "Bạn có chắc chắn muốn xóa bài tập này?"
3. Nếu YES:
   ├─ Gửi DELETE request qua AJAX
   ├─ Xóa bài tập và tất cả bài nộp liên quan
   ├─ Xóa file đính kèm trên server
   └─ Reload trang với thông báo thành công
4. Nếu NO: Hủy thao tác
```

**Controller Action**: `BaiTapController.Xoa(classId, assignmentId)`

**API Call**: `DELETE /api/assignment/{classId}/{assignmentId}`

**JavaScript Function**: `deleteAssignment(classId, assignmentId)`

---

### 5️⃣ XEM DANH SÁCH BÀI NỘP

**URL**: `/bai-tap/{assignmentId}/danh-sach-bai-nop`

**Các Bước:**
```
1. Từ danh sách bài tập, click menu dropdown → "Xem Bài Nộp"
   HOẶC từ chi tiết bài tập, click "Xem Bài Nộp"
2. Hệ thống hiển thị:
   ├─ Thống kê:
   │  ├─ Tổng số bài nộp
   │  ├─ Số bài đã chấm
   │  ├─ Số bài nộp muộn
   │  └─ Điểm trung bình
   └─ Bảng danh sách bài nộp:
      ├─ STT
      ├─ Thông tin nhân viên (Tên, ID, Email)
      ├─ Thời gian nộp
      ├─ Trạng thái (Đúng hạn/Nộp muộn)
      ├─ Điểm (nếu đã chấm)
      └─ Thao tác (Xem chi tiết, Chấm điểm)
3. Mentor có thể:
   ├─ Xem chi tiết từng bài nộp
   ├─ Chấm điểm nhanh qua modal
   └─ Filter/Sort danh sách
```

**Controller Action**: `BaiTapController.DanhSachBaiNop(assignmentId)`

**API Call**: `GET /api/assignment/{assignmentId}/submissions`

**Response Data**:
```json
[
  {
    "submissionId": 1,
    "employeeId": "NV001",
    "employeeName": "Nguyễn Văn A",
    "employeeEmail": "nva@company.com",
    "submittedAt": "2025-11-20T14:30:00",
    "isLate": false,
    "score": 8.5,
    "isGraded": true
  }
]
```

---

### 6️⃣ CHẤM ĐIỂM BÀI NỘP

**URL**: `/bai-tap/{assignmentId}/bai-nop/{submissionId}/cham-diem` (AJAX)

**Cách 1: Chấm điểm từ danh sách (Modal)**
```
1. Từ danh sách bài nộp, click nút "Chấm điểm"
2. Hiển thị modal với:
   ├─ Tên nhân viên
   ├─ Input điểm (0-10)
   └─ Textarea nhận xét
3. Nhập điểm và nhận xét
4. Click "Lưu Điểm"
5. Gửi request qua AJAX
6. Reload trang với thông báo thành công
```

**Cách 2: Chấm điểm từ chi tiết bài nộp (Inline Form)**
```
1. Click "Xem chi tiết" bài nộp
2. Hệ thống hiển thị:
   ├─ Thông tin nhân viên
   ├─ File đã nộp (có thể download)
   ├─ Ghi chú của học viên
   └─ Form chấm điểm (nếu chưa chấm)
3. Nhập điểm và nhận xét
4. Click "Lưu Điểm"
5. Reload trang với kết quả chấm điểm
```

**Controller Action**: `BaiTapController.ChamDiem(assignmentId, submissionId, GradeSubmissionDto)`

**API Call**: `PUT /api/assignment/{assignmentId}/submissions/{submissionId}/grade`

**Request Body**:
```json
{
  "score": 8.5,
  "feedback": "Bài làm tốt, cần cải thiện phần X"
}
```

**JavaScript Function**: `gradeSubmission(assignmentId, submissionId, score, feedback)`

---

### 7️⃣ XEM CHI TIẾT BÀI NỘP

**URL**: `/bai-tap/{assignmentId}/bai-nop/{submissionId}`

**Các Bước:**
```
1. Click vào bài nộp từ danh sách
2. Hệ thống hiển thị:
   ├─ Thông tin nhân viên:
   │  ├─ Tên, ID, Email
   │  └─ Avatar
   ├─ Thông tin bài nộp:
   │  ├─ Thời gian nộp
   │  ├─ Trạng thái (Đúng hạn/Muộn)
   │  └─ Ghi chú (nếu có)
   ├─ File đã nộp:
   │  ├─ Tên file
   │  ├─ Kích thước
   │  └─ Nút download
   ├─ Kết quả chấm điểm (nếu có):
   │  ├─ Điểm số
   │  ├─ Nhận xét
   │  ├─ Người chấm
   │  └─ Thời gian chấm
   └─ Form chấm điểm (nếu chưa chấm)
```

**Controller Action**: `BaiTapController.ChiTietBaiNop(assignmentId, submissionId)`

**API Call**: `GET /api/assignment/{assignmentId}/submissions/{submissionId}`

---

## 👨‍💼 LUỒNG CỦA STAFF

### 1️⃣ XEM DANH SÁCH BÀI TẬP

**URL**: `/bai-tap/{classId}`

**Các Bước:**
```
1. Staff truy cập "Lớp Học Của Tôi"
2. Click vào lớp học đang tham gia
3. Click vào "Bài Tập" hoặc URL /bai-tap/{classId}
4. Hệ thống hiển thị:
   ├─ Chỉ các bài tập của lớp mà Staff tham gia
   ├─ Thông tin mỗi bài tập:
   │  ├─ Tiêu đề
   │  ├─ Mô tả ngắn
   │  ├─ Hạn nộp
   │  ├─ Trạng thái (Còn X ngày, Quá hạn)
   │  └─ File đính kèm (nếu có)
   └─ Nút "Xem Chi Tiết"
5. Staff KHÔNG thể tạo/sửa/xóa bài tập
```

**Controller Action**: `BaiTapController.Index(classId)`

**API Call**: `GET /api/assignment/{classId}`

---

### 2️⃣ XEM CHI TIẾT BÀI TẬP

**URL**: `/bai-tap/{classId}/chi-tiet/{assignmentId}`

**Các Bước:**
```
1. Click "Xem Chi Tiết" từ danh sách
2. Hệ thống hiển thị:
   ├─ Thông tin bài tập:
   │  ├─ Tiêu đề
   │  ├─ Mô tả đầy đủ
   │  ├─ Hạn nộp
   │  ├─ Trạng thái (Còn X ngày/Quá hạn)
   │  └─ File đính kèm (có thể download)
   ├─ Sidebar thông tin:
   │  ├─ ID bài tập
   │  ├─ Ngày tạo
   │  ├─ Hạn nộp
   │  └─ Lớp học
   └─ Form nộp bài (cho Staff):
      ├─ Input chọn file (*)
      ├─ Textarea ghi chú (optional)
      └─ Nút "Nộp Bài"
3. Staff có thể:
   ├─ Download file đính kèm của bài tập
   └─ Nộp bài làm
```

**Controller Action**: `BaiTapController.ChiTiet(classId, assignmentId)`

**API Call**: `GET /api/assignment/{classId}/{assignmentId}`

---

### 3️⃣ NỘP BÀI TẬP

**URL**: `/bai-tap/{assignmentId}/nop-bai` (AJAX)

**Các Bước:**
```
1. Từ chi tiết bài tập, scroll đến form "Nộp Bài Tập"
2. Click "Chọn File" và chọn file bài làm
3. Hệ thống validate:
   ├─ File không được rỗng
   └─ Kích thước ≤ 100MB
4. (Optional) Nhập ghi chú cho bài nộp
5. Click "Nộp Bài"
6. Hệ thống:
   ├─ Hiển thị loading spinner
   ├─ Upload file lên server
   ├─ Lưu thông tin bài nộp:
   │  ├─ AssignmentId
   │  ├─ EmployeeId (từ token)
   │  ├─ File (tên, URL, size, mime type)
   │  ├─ Note
   │  ├─ SubmittedAt (thời gian nộp)
   │  └─ IsLate (so sánh với DueDate)
   └─ Reload trang với thông báo:
      "Nộp bài thành công!"
7. Lưu ý:
   ├─ Staff có thể nộp lại (file mới sẽ thay thế file cũ)
   ├─ Mỗi lần nộp chỉ được 1 file
   └─ Có thể nộp sau hạn (sẽ đánh dấu "Nộp muộn")
```

**Controller Action**: `BaiTapController.NopBai(assignmentId, SubmitAssignmentForm)`

**API Call**: `POST /api/assignment/{assignmentId}/submissions`

**Form Data**:
```
- File: IFormFile (required)
- Note: string (optional)
```

**JavaScript Function**: `initSubmitAssignmentForm(assignmentId, classId)`

**Validation**:
- File size ≤ 100MB
- File type: Tất cả các loại file
- MIME type sẽ được lưu lại

---

### 4️⃣ XEM KẾT QUẢ CHẤM ĐIỂM

**URL**: `/bai-tap/{assignmentId}/bai-nop/{submissionId}`

**Các Bước:**
```
1. Sau khi nộp bài, Staff có thể xem lại bài nộp của mình
2. Hệ thống hiển thị:
   ├─ Thông tin bài nộp:
   │  ├─ Thời gian nộp
   │  ├─ Trạng thái (Đúng hạn/Muộn)
   │  └─ Ghi chú đã gửi
   ├─ File đã nộp:
   │  ├─ Tên file
   │  ├─ Kích thước
   │  └─ Nút download (xem lại bài làm)
   └─ Kết quả chấm điểm (nếu Mentor đã chấm):
      ├─ Điểm số (highlight lớn)
      ├─ Nhận xét của Mentor
      ├─ Người chấm
      └─ Thời gian chấm
3. Nếu chưa được chấm:
   └─ Hiển thị: "Chưa chấm điểm - Bài nộp đang chờ Mentor chấm điểm"
```

**Controller Action**: `BaiTapController.ChiTietBaiNop(assignmentId, submissionId)`

**API Call**: `GET /api/assignment/{assignmentId}/submissions/{submissionId}`

---

## 📊 SƠ ĐỒ LUỒNG

### Luồng Tổng Quát

```
┌─────────────────────────────────────────────────────────────────┐
│                         QUẢN LÝ BÀI TẬP                          │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
                    ┌──────────────────┐
                    │   Đăng nhập      │
                    └──────────────────┘
                              │
                ┌─────────────┴─────────────┐
                ▼                           ▼
         ┌────────────┐              ┌────────────┐
         │   MENTOR   │              │   STAFF    │
         └────────────┘              └────────────┘
                │                           │
    ┌───────────┼───────────┐              │
    ▼           ▼           ▼              ▼
┌────────┐ ┌────────┐ ┌────────┐    ┌──────────┐
│Tạo BT  │ │Sửa BT  │ │Xóa BT  │    │Xem DS BT │
└────────┘ └────────┘ └────────┘    └──────────┘
    │           │           │              │
    └───────────┴───────────┘              ▼
                │                    ┌──────────┐
                ▼                    │Xem Chi   │
        ┌──────────────┐             │Tiết BT   │
        │Xem DS Bài Nộp│             └──────────┘
        └──────────────┘                   │
                │                          ▼
                ▼                    ┌──────────┐
        ┌──────────────┐             │ Nộp Bài  │
        │  Chấm Điểm   │             └──────────┘
        └──────────────┘                   │
                                           ▼
                                     ┌──────────┐
                                     │Xem Điểm  │
                                     └──────────┘
```

### Luồng Chi Tiết - Mentor Tạo Bài Tập

```
START
  │
  ▼
┌─────────────────────────┐
│ Truy cập DS bài tập     │
│ /bai-tap/{classId}      │
└─────────────────────────┘
  │
  ▼
┌─────────────────────────┐
│ Click "Tạo Bài Tập Mới" │
└─────────────────────────┘
  │
  ▼
┌─────────────────────────┐
│ Điền form:              │
│ - Tiêu đề               │
│ - Mô tả                 │
│ - Hạn nộp               │
│ - File đính kèm         │
└─────────────────────────┘
  │
  ▼
┌─────────────────────────┐
│ Validate dữ liệu        │
└─────────────────────────┘
  │
  ├─[Lỗi]───────────────────┐
  │                          │
  ▼                          ▼
[OK]                  ┌─────────────┐
  │                   │ Hiển thị lỗi│
  ▼                   └─────────────┘
┌─────────────────────────┐        │
│ Upload file (nếu có)    │        │
└─────────────────────────┘        │
  │                                 │
  ▼                                 │
┌─────────────────────────┐        │
│ Lưu vào DB              │        │
│ POST /api/assignment    │        │
└─────────────────────────┘        │
  │                                 │
  ▼                                 │
┌─────────────────────────┐        │
│ Redirect về danh sách   │        │
│ với thông báo thành công│        │
└─────────────────────────┘        │
  │                                 │
  ▼                                 │
END◄──────────────────────────────┘
```

### Luồng Chi Tiết - Staff Nộp Bài

```
START
  │
  ▼
┌─────────────────────────┐
│ Truy cập chi tiết BT    │
│ /bai-tap/{classId}/     │
│ chi-tiet/{assignmentId} │
└─────────────────────────┘
  │
  ▼
┌─────────────────────────┐
│ Xem yêu cầu bài tập     │
│ - Mô tả                 │
│ - Hạn nộp               │
│ - File hướng dẫn        │
└─────────────────────────┘
  │
  ▼
┌─────────────────────────┐
│ Scroll đến form nộp bài │
└─────────────────────────┘
  │
  ▼
┌─────────────────────────┐
│ Chọn file bài làm       │
└─────────────────────────┘
  │
  ▼
┌─────────────────────────┐
│ Validate file           │
│ - Không rỗng            │
│ - Size ≤ 100MB          │
└─────────────────────────┘
  │
  ├─[Lỗi]───────────────────┐
  │                          │
  ▼                          ▼
[OK]                  ┌─────────────┐
  │                   │Alert lỗi    │
  ▼                   └─────────────┘
┌─────────────────────────┐        │
│ (Optional) Nhập ghi chú │        │
└─────────────────────────┘        │
  │                                 │
  ▼                                 │
┌─────────────────────────┐        │
│ Click "Nộp Bài"         │        │
└─────────────────────────┘        │
  │                                 │
  ▼                                 │
┌─────────────────────────┐        │
│ Upload file qua AJAX    │        │
│ POST /api/assignment/   │        │
│ {id}/submissions        │        │
└─────────────────────────┘        │
  │                                 │
  ▼                                 │
┌─────────────────────────┐        │
│ Server lưu:             │        │
│ - File                  │        │
│ - Metadata              │        │
│ - Thời gian nộp         │        │
│ - Đánh dấu muộn/đúng hạn│        │
└─────────────────────────┘        │
  │                                 │
  ▼                                 │
┌─────────────────────────┐        │
│ Reload trang            │        │
│ Hiển thị thông báo      │        │
│ "Nộp bài thành công!"   │        │
└─────────────────────────┘        │
  │                                 │
  ▼                                 │
END◄──────────────────────────────┘
```

### Luồng Chi Tiết - Mentor Chấm Điểm

```
START
  │
  ▼
┌─────────────────────────┐
│ Xem danh sách bài nộp   │
│ /bai-tap/{assignmentId}/│
│ danh-sach-bai-nop       │
└─────────────────────────┘
  │
  ▼
┌─────────────────────────┐
│ Chọn bài nộp cần chấm   │
└─────────────────────────┘
  │
  ├─────────────────┬──────────────────┐
  ▼                 ▼                  ▼
[Modal]      [Chi tiết]         [Inline form]
  │                 │                  │
  ▼                 ▼                  │
┌─────────────────────────┐           │
│ Click "Chấm điểm"       │           │
│ trong danh sách         │           │
└─────────────────────────┘           │
  │                 │                  │
  ▼                 ▼                  │
┌─────────────────────────┐           │
│ Hiển thị modal/form     │◄──────────┘
│ - Tên NV                │
│ - Input điểm (0-10)     │
│ - Textarea nhận xét     │
└─────────────────────────┘
  │
  ▼
┌─────────────────────────┐
│ Nhập điểm và nhận xét   │
└─────────────────────────┘
  │
  ▼
┌─────────────────────────┐
│ Validate:               │
│ - Điểm: 0 ≤ score ≤ 10  │
└─────────────────────────┘
  │
  ├─[Lỗi]───────────────────┐
  │                          │
  ▼                          ▼
[OK]                  ┌─────────────┐
  │                   │Alert lỗi    │
  ▼                   └─────────────┘
┌─────────────────────────┐        │
│ Click "Lưu Điểm"        │        │
└─────────────────────────┘        │
  │                                 │
  ▼                                 │
┌─────────────────────────┐        │
│ Gửi request qua AJAX    │        │
│ PUT /api/assignment/    │        │
│ {id}/submissions/{id}/  │        │
│ grade                   │        │
└─────────────────────────┘        │
  │                                 │
  ▼                                 │
┌─────────────────────────┐        │
│ Server lưu:             │        │
│ - Score                 │        │
│ - Feedback              │        │
│ - GradedBy (mentorId)   │        │
│ - GradedAt (timestamp)  │        │
└─────────────────────────┘        │
  │                                 │
  ▼                                 │
┌─────────────────────────┐        │
│ Reload trang            │        │
│ Cập nhật điểm trong DS  │        │
└─────────────────────────┘        │
  │                                 │
  ▼                                 │
┌─────────────────────────┐        │
│ Staff có thể xem điểm   │        │
│ và nhận xét             │        │
└─────────────────────────┘        │
  │                                 │
  ▼                                 │
END◄──────────────────────────────┘
```

---

## 🔌 API ENDPOINTS

### 1. Danh Sách Bài Tập

**Endpoint**: `GET /api/assignment/{classId}`

**Authorization**: 
- Mentor: Xem tất cả bài tập
- Staff: Chỉ xem nếu thuộc lớp

**Response**:
```json
[
  {
    "assignmentId": 1,
    "classId": 5,
    "className": "SE1801_CS1",
    "title": "Bài tập tuần 1",
    "description": "Làm bài tập về C# basics",
    "dueDate": "2025-12-05T23:59:00",
    "createdAt": "2025-11-20T10:00:00",
    "attachmentFileName": "huong-dan.pdf",
    "attachmentUrl": "https://storage/assignments/1/huong-dan.pdf",
    "attachmentMimeType": "application/pdf",
    "attachmentSizeBytes": 2048576
  }
]
```

---

### 2. Chi Tiết Bài Tập

**Endpoint**: `GET /api/assignment/{classId}/{assignmentId}`

**Authorization**: Mentor hoặc Staff thuộc lớp

**Response**: Same as above (single object)

---

### 3. Tạo Bài Tập

**Endpoint**: `POST /api/assignment/{classId}`

**Authorization**: Mentor only

**Request** (multipart/form-data):
```
ClassId: 5
Title: "Bài tập tuần 1"
Description: "Làm bài tập về C# basics"
DueDate: "2025-12-05T23:59:00"
AttachmentFile: [binary file]
```

**Response**: Created AssignmentDto (201 Created)

---

### 4. Cập Nhật Bài Tập

**Endpoint**: `PUT /api/assignment/{classId}/{assignmentId}`

**Authorization**: Mentor only

**Request** (multipart/form-data):
```
Title: "Bài tập tuần 1 (Updated)"
Description: "Cập nhật mô tả"
DueDate: "2025-12-06T23:59:00"
RemoveAttachment: false
AttachmentFile: [binary file] (optional)
```

**Response**: Updated AssignmentDto

---

### 5. Xóa Bài Tập

**Endpoint**: `DELETE /api/assignment/{classId}/{assignmentId}`

**Authorization**: Mentor only

**Response**: 204 No Content

---

### 6. Danh Sách Bài Nộp

**Endpoint**: `GET /api/assignment/{assignmentId}/submissions`

**Authorization**: Mentor only

**Response**:
```json
[
  {
    "submissionId": 1,
    "employeeId": "NV001",
    "employeeName": "Nguyễn Văn A",
    "employeeEmail": "nva@company.com",
    "submittedAt": "2025-11-25T14:30:00",
    "isLate": false,
    "score": 8.5,
    "isGraded": true
  }
]
```

---

### 7. Chi Tiết Bài Nộp

**Endpoint**: `GET /api/assignment/{assignmentId}/submissions/{submissionId}`

**Authorization**: 
- Mentor: Xem tất cả
- Staff: Chỉ xem bài nộp của mình

**Response**:
```json
{
  "submissionId": 1,
  "assignmentId": 1,
  "assignmentTitle": "Bài tập tuần 1",
  "employeeId": "NV001",
  "employeeName": "Nguyễn Văn A",
  "employeeEmail": "nva@company.com",
  "submittedAt": "2025-11-25T14:30:00",
  "isLate": false,
  "note": "Em đã hoàn thành bài tập",
  "fileName": "bai-lam.zip",
  "fileUrl": "https://storage/submissions/1/bai-lam.zip",
  "fileMimeType": "application/zip",
  "fileSizeBytes": 5242880,
  "score": 8.5,
  "feedback": "Bài làm tốt, cần cải thiện phần X",
  "gradedAt": "2025-11-26T10:00:00",
  "gradedByName": "Thầy Nguyễn Văn B"
}
```

---

### 8. Nộp Bài

**Endpoint**: `POST /api/assignment/{assignmentId}/submissions`

**Authorization**: Staff only

**Request** (multipart/form-data):
```
File: [binary file]
Note: "Em đã hoàn thành bài tập" (optional)
```

**Response**: Created SubmissionDetailDto (201 Created)

**Notes**:
- Mỗi lần nộp chỉ 1 file
- Nếu nộp lại, file mới sẽ thay thế file cũ
- Max size: 100MB

---

### 9. Chấm Điểm

**Endpoint**: `PUT /api/assignment/{assignmentId}/submissions/{submissionId}/grade`

**Authorization**: Mentor only

**Request** (application/json):
```json
{
  "score": 8.5,
  "feedback": "Bài làm tốt, cần cải thiện phần X"
}
```

**Response**: Updated SubmissionDetailDto

---

## 📝 BUSINESS RULES

### Bài Tập (Assignment)

1. **Tạo Bài Tập**
   - Chỉ Mentor của lớp mới được tạo
   - Tiêu đề bắt buộc, tối đa 200 ký tự
   - Hạn nộp phải là thời điểm tương lai
   - File đính kèm tối đa 100MB

2. **Sửa Bài Tập**
   - Chỉ Mentor của lớp mới được sửa
   - Có thể sửa cả khi đã có bài nộp
   - Có thể xóa hoặc thay file đính kèm

3. **Xóa Bài Tập**
   - Chỉ Mentor của lớp mới được xóa
   - Xóa bài tập sẽ xóa luôn tất cả bài nộp
   - Xóa file đính kèm trên server

### Bài Nộp (Submission)

1. **Nộp Bài**
   - Chỉ Staff thuộc lớp mới được nộp
   - Mỗi lần nộp chỉ 1 file
   - File bắt buộc, tối đa 100MB
   - Có thể nộp sau hạn (đánh dấu "Nộp muộn")
   - Có thể nộp lại nhiều lần (file mới thay thế file cũ)

2. **Chấm Điểm**
   - Chỉ Mentor của lớp mới được chấm
   - Điểm từ 0 đến 10
   - Nhận xét không bắt buộc
   - Có thể chấm lại (cập nhật điểm)

3. **Xem Bài Nộp**
   - Mentor: Xem tất cả bài nộp của lớp
   - Staff: Chỉ xem bài nộp của mình

---

## ⚠️ VALIDATION & ERROR HANDLING

### Client-Side Validation

1. **Form Tạo/Sửa Bài Tập**
   ```javascript
   - Tiêu đề không rỗng
   - Hạn nộp phải > hiện tại
   - File ≤ 100MB (nếu có)
   ```

2. **Form Nộp Bài**
   ```javascript
   - File không rỗng
   - File ≤ 100MB
   ```

3. **Form Chấm Điểm**
   ```javascript
   - Điểm: 0 ≤ score ≤ 10
   - Điểm phải là số
   ```

### Server-Side Validation

1. **Authentication & Authorization**
   ```csharp
   - Token hợp lệ
   - Role phù hợp (Mentor/Staff)
   - Thuộc lớp học
   ```

2. **Business Logic**
   ```csharp
   - Bài tập tồn tại
   - Lớp học tồn tại
   - Hạn nộp hợp lệ
   - File size hợp lệ
   ```

### Error Messages

```javascript
// Client
"Vui lòng nhập tiêu đề bài tập!"
"Hạn nộp phải là thời điểm trong tương lai!"
"Kích thước file không được vượt quá 100MB!"
"Vui lòng chọn file để nộp!"
"Điểm phải từ 0 đến 10!"

// Server
"Không tìm thấy bài tập."
"Bạn không có quyền thực hiện thao tác này."
"Phiên đăng nhập đã hết hạn."
"Dữ liệu không hợp lệ."
```

---

## 🎨 UI/UX NOTES

### Design Principles

1. **Responsive**: Tất cả màn hình đều responsive cho mobile
2. **Loading States**: Hiển thị spinner khi upload/submit
3. **Feedback**: Alert success/error rõ ràng
4. **Confirmation**: Confirm trước khi xóa
5. **File Preview**: Hiển thị thông tin file trước khi upload

### Color Coding

- 🟢 **Xanh lá**: Đúng hạn, Đã chấm điểm
- 🟡 **Vàng**: Còn ít ngày (≤3 ngày), Nộp muộn
- 🔴 **Đỏ**: Quá hạn
- 🔵 **Xanh dương**: Thông tin, Điểm số
- ⚪ **Xám**: Chưa chấm điểm

### Icons (Font Awesome)

```
fa-tasks         - Bài tập
fa-file-alt      - Chi tiết
fa-plus-circle   - Tạo mới
fa-edit          - Sửa
fa-trash         - Xóa
fa-upload        - Nộp bài
fa-download      - Tải xuống
fa-star          - Chấm điểm
fa-check-circle  - Thành công
fa-exclamation   - Cảnh báo
```

---

## 🚀 TESTING SCENARIOS

### Test Case 1: Mentor Tạo Bài Tập
```
1. Login as Mentor
2. Navigate to /bai-tap/{classId}
3. Click "Tạo Bài Tập Mới"
4. Fill form with valid data
5. Upload file (optional)
6. Submit
Expected: Redirect to list with success message
```

### Test Case 2: Staff Nộp Bài
```
1. Login as Staff
2. Navigate to /bai-tap/{classId}/chi-tiet/{assignmentId}
3. Choose file (<100MB)
4. Add note (optional)
5. Click "Nộp Bài"
Expected: Success message, page reload with submission info
```

### Test Case 3: Mentor Chấm Điểm
```
1. Login as Mentor
2. Navigate to /bai-tap/{assignmentId}/danh-sach-bai-nop
3. Click "Chấm điểm" on ungraded submission
4. Enter score (0-10) and feedback
5. Submit
Expected: Success message, score updated in list
```

### Edge Cases

1. **Upload file > 100MB**: Alert error "Kích thước file quá lớn"
2. **Nộp bài sau deadline**: Mark as "Nộp muộn" but still accept
3. **Xóa bài tập có bài nộp**: Confirm + Delete all submissions
4. **Staff xem bài tập của lớp khác**: 403 Forbidden
5. **Nộp bài lại**: Replace old submission

---

## 📚 RELATED DOCUMENTATION

- [API Backend Documentation](../InternalTrainingSystem.API/README.md)
- [Database Schema](../InternalTrainingSystem.API/DB/README.md)
- [User Roles & Permissions](../ROLES.md)
- [File Storage Service](../STORAGE.md)

---

## 🔄 VERSION HISTORY

- **v1.0** (2025-11-27): Initial documentation
  - Tạo/Sửa/Xóa bài tập
  - Nộp bài
  - Chấm điểm
  - Upload file đính kèm

---

**Ghi chú**: Tài liệu này mô tả luồng hoạt động của tính năng quản lý bài tập. Các màn hình và API đã được implement đầy đủ theo mô tả trên.
