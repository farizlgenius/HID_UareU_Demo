# 📷 WPF Face Recognition App (UareU Camera HID)

A modern **WPF (.NET C#)** desktop application that integrates with **UareU Camera HID** for automatic face capture and recognition workflows.

This project demonstrates a complete face recognition pipeline including capture, verification, matching, and identification.

---

## 🖼️ App Preview

> Replace these mockup images with real screenshots later.

### 🏠 Main Workflow
<img width="1914" height="1145" alt="image" src="https://github.com/user-attachments/assets/11211497-2e97-4a30-a076-7635973ec84f" />


### 📸 Auto Capture
<img width="1902" height="1144" alt="image" src="https://github.com/user-attachments/assets/2cbbe5ea-472d-4ed3-af4c-0fb8c279d8ed" />

### Verification
<img width="1905" height="1145" alt="image" src="https://github.com/user-attachments/assets/c702af98-6d4d-4afd-ac79-665789bc52c6" />

### Enrollment
<img width="1906" height="1143" alt="image" src="https://github.com/user-attachments/assets/bcbe2036-e527-4b5a-b8b9-6ccd7d7ddd7e" />

### 🔍 Identification
<img width="1907" height="1142" alt="image" src="https://github.com/user-attachments/assets/9359fe28-4756-446a-afea-4f06557d1e19" />


### Database 
<img width="1902" height="1140" alt="image" src="https://github.com/user-attachments/assets/8505bc31-9e08-4a9b-b4a9-7ea92e853498" />



---

## ✨ Features

### 🤖 Auto Capture
Automatically detects a face from live camera stream and captures the image without user interaction.

**Features**
- Live camera streaming
- Automatic face detection
- Auto snapshot when face is found
- Real-time FPS display

---

### 🖼️ Image Match
Compare two images and calculate **face similarity score**.

**Features**
- Upload reference image
- Capture live image
- Display match score
- Visual match / unmatch indicator

---

### ✅ Verify Image With Live Camera
Verify a person by comparing:
- Stored image
- Live camera image

**Features**
- Real-time verification
- Match confidence score
- Visual status feedback

---

### 🧠 Identify From Database
Identify a person from all enrolled records using the live camera.

**Features**
- Automatic identification
- Highest score detection
- Display name + confidence score
- Match / No Match UI status

---

### 🗄️ Database Viewer
View all enrolled face profiles stored in the system.

**Features**
- List of registered users
- Stored face images
- Employee information

---

## 🏗️ Tech Stack

- WPF (.NET)
- C#
- Accord.Video / DirectShow
- UareU Camera HID
- LINQ
- Modern WPF UI Design

---

## 🚀 Application Flow

```
Camera Start → Detect Face → Capture → Match / Verify / Identify → Save → Database
```

---

## 📂 Project Modules

| Module | Description |
|---|---|
| AutoCapturePage | Automatic face capture |
| MatchingPage | Image vs Image comparison |
| VerificationPage | Stored image vs Live camera |
| IdentificationPage | Identify from database |
| DatabasePage | View all enrolled faces |

---

## 🛠️ Setup

1. Connect **UareU Camera HID**
2. Run the WPF application
3. Camera streaming starts automatically
4. Start capturing & recognizing faces 🎉

---

## 📌 Notes

- Replace images inside `/docs` folder with real screenshots.
- Designed for internal biometric / identity workflow usage.
