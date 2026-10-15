# LOST_FIND
### 지역문제해결 미니 캡스톤
---

1. 프로젝트 소개

   대중교통에서 잃어버린 분실물 및 습득물 발견 시 원래 주인이 원활하게 수령할 수 있도록 관리하고 매칭하는 웹(앱) 프로그램

2. 개발 기간

   2026.09 ~ 2026.12

3. 프로젝트 구성
   
   프론트엔드 - HTML, CSS, JavaScript
   백엔드 - C#
   DB - MySQL
   (웹앱 구현)


# 찾아드림

서울특별시 중랑구를 중심으로 분실물과 습득물 정보를 검색하고 신고할 수 있도록 제작한 팀 프로젝트 초안입니다.
서울 열린데이터광장과 (미정/경찰청 LOST112 공공데이터 연동을 목표로 하며), 사용자 직접 등록 정보와 공공데이터를 함께 제공하는 구조입니다.

## 주요 기능

- 분실물·습득물 신고 화면
- 물품명, 분류, 지역, 상태 검색 및 필터
- 서울 열린데이터광장 습득물 정보 조회
- LOST112 습득물 API 연동 구조
- 중복 공공데이터 통합 및 빈 정보 상호 보완
- 보관 중 물품과 주인이 찾아간 물품 분류
- 전체 습득물 전용 페이지
- 실종 반려동물 전용 신고 화면
- 회원가입·로그인 UI 및 백엔드 API 초안
- 문의, 북마크, 처리 완료 기능 초안
- PC·태블릿·모바일 반응형 화면

## 기술 구성

| 구분 | 기술 |
| --- | --- |
| 프론트엔드 | HTML5, CSS3, Vanilla JavaScript |
| 백엔드 | C#, ASP.NET Core Minimal API |
| 실행 환경 | .NET 10 |
| 공공데이터 | 서울 열린데이터광장, 경찰청 LOST112 |
| 배포 | Docker, Cloudtype |
| 예정 데이터베이스 | MySQL 또는 MariaDB |

## 프로젝트 구조

```text
lostfind/
├─ dist/
│  ├─ index.html          # 메인 화면
│  ├─ items.html          # 전체 습득물 화면
│  └─ assets/             # 이미지 파일
├─ Models/
│  ├─ LostItemModels.cs
│  └─ SeoulOpenDataModels.cs
├─ Program.cs             # ASP.NET Core 서버와 API
├─ LostFind.csproj
├─ Dockerfile
└─ README.md
```

## 로컬 실행 방법

### 준비 사항

- .NET 10 SDK
- Git

### 실행

프로젝트 폴더에서 다음 명령을 실행합니다.

```powershell
dotnet restore
dotnet run
```

## 환경변수

| 변수명 | 설명 | 필수 여부 |
| --- | --- | --- |
| `SEOUL_OPEN_API_KEY` | 서울 열린데이터광장 인증키 | 배포 시 필수 |
| `SEOUL_OPEN_API_BASE_URL` | 서울 열린데이터광장 기본 주소 | 선택 |
| `DATA_GO_KR_API_KEY` | LOST112용 공공데이터포털 인증키 | LOST112 사용 시 필수 |
| `LOST112_API_KEY` | LOST112 인증키 대체 변수 | 선택 |
| `LOST112_API_BASE_URL` | LOST112 API 주소 | 선택 |

PowerShell에서 임시 환경변수를 설정하는 예시는 다음과 같습니다.

```powershell
$env:SEOUL_OPEN_API_KEY="발급받은_서울시_API_키"
$env:DATA_GO_KR_API_KEY="발급받은_공공데이터포털_API_키"
dotnet run
```

인증키와 비밀번호는 GitHub에 올리지 말고 로컬 환경변수나 Cloudtype 환경변수로 관리해야 합니다.

## 주요 API

| 메서드 | 경로 | 설명 |
| --- | --- | --- |
| `GET` | `/api/public-items` | 공공데이터 습득물 목록 조회 |
| `GET` | `/api/items` | 사용자 등록 물품 목록 조회 |
| `POST` | `/api/items` | 분실물·습득물 등록 |
| `PUT` | `/api/items/{id}/status` | 처리 상태 변경 |
| `POST` | `/api/items/{id}/bookmark` | 북마크 변경 |
| `POST` | `/api/items/{id}/inquiries` | 문의 등록 |
| `GET` | `/api/pets` | 실종 반려동물 목록 조회 |
| `POST` | `/api/pets` | 실종 반려동물 신고 |
| `POST` | `/api/auth/login` | 로그인 초안 |
| `POST` | `/api/auth/register` | 회원가입 초안 |

공공데이터 조회 예시:

```text
GET /api/public-items?start=1&end=100
```

강제 최신화:

```text
GET /api/public-items?start=1&end=100&refresh=true
```

## 공공데이터 처리 방식

- 2025년 이전 자료는 백엔드 응답에서 제외합니다.
- 서울시 자료와 LOST112 자료는 동시에 조회합니다.
- 관리번호가 같으면 동일 물품으로 판단합니다.
- 관리번호가 다르면 물품명, 등록일, 보관장소를 기준으로 중복을 확인합니다.
- 중복 자료는 하나로 합치고 비어 있는 정보만 보완합니다.
- 정상 조회 결과는 서버 메모리에 10분 동안 저장합니다.
- 외부 API별 최대 대기시간은 5초입니다.

## Docker 실행

```powershell
docker build -t lostfind .
docker run --rm -p 8080:8080 `
  -e SEOUL_OPEN_API_KEY="서울시_API_키" `
  -e DATA_GO_KR_API_KEY="공공데이터포털_API_키" `
  lostfind
```

브라우저에서 `http://127.0.0.1:8080`으로 접속합니다.

## Cloudtype 배포 설정

- 배포 방식: `Dockerfile`
- 공개 포트: `8080`
- Dockerfile 경로: `Dockerfile`
- 환경변수: `SEOUL_OPEN_API_KEY`, `DATA_GO_KR_API_KEY`

Cloudtype 환경변수에 인증키를 등록한 후 GitHub 저장소와 연결하여 배포합니다.

## 현재 개발 단계의 제한사항

- 사용자 등록 자료는 현재 서버 메모리에 저장되어 서버 재시작 시 사라집니다.
- 로그인과 회원가입은 실제 인증 시스템이 아닌 기능 초안입니다.
- 사진 파일은 영구 스토리지에 저장되지 않습니다.
- LOST112 자료를 사용하려면 별도의 공공데이터포털 인증키가 필요합니다.
- 공공데이터 캐시는 서버 메모리 방식이므로 서버 재시작 시 초기화됩니다.
- MySQL 또는 MariaDB 연결은 아직 구현되지 않았습니다.

## 추후 구현 예정

- MySQL/MariaDB 영구 저장
- 실제 회원 인증과 사용자·관리자 권한 구분
- 사진 업로드 스토리지 연결
- 공공데이터 주기적 백그라운드 갱신
- 마지막 정상 공공데이터 보존
- 분실물·습득물 자동 매칭 개선
- 문의 또는 채팅 기능
- 관리자 신고·게시물 관리 기능

## 팀 작업 권장 방식

기능별 브랜치를 생성하여 작업한 후 검토를 거쳐 `main` 브랜치에 병합합니다.

```text
feature/login
feature/database
feature/item-register
feature/search-filter
feature/public-data
feature/matching
```

