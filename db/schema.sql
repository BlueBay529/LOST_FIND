-- =====================================================================
-- 분실물 찾기 도우미 - DB 생성 스크립트 (MySQL 8.0)
-- 실행 순서: members -> keepers -> lost_items -> found_items
-- =====================================================================

-- 처음부터 다시 만들고 싶을 때만 아래 줄의 주석(--)을 지우고 실행하세요. (모든 데이터 삭제)
-- DROP DATABASE IF EXISTS lostfind;

CREATE DATABASE IF NOT EXISTS lostfind
  DEFAULT CHARACTER SET utf8mb4;

USE lostfind;

-- ---------------------------------------------------------------------
-- 1. members / 회원
-- ---------------------------------------------------------------------
CREATE TABLE members (
  user_primary_id INT          NOT NULL AUTO_INCREMENT,   -- 회원가입 시 자동 부여되는 고유 아이디
  user_input_id   VARCHAR(50)  NOT NULL,                  -- 사용자가 입력하는 아이디 (로그인용)
  password_hash   VARCHAR(255) NOT NULL,                  -- 비밀번호 해시 (BCrypt 등)
  user_name       VARCHAR(10)  NOT NULL,
  user_tel        VARCHAR(15)  NOT NULL,
  created_at      DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (user_primary_id),
  UNIQUE KEY uq_members_input_id (user_input_id)
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------
-- 2. keepers / 보관기관
-- ---------------------------------------------------------------------
CREATE TABLE keepers (
  keeper_id       VARCHAR(50)  NOT NULL,                  -- 보관기관 고유번호
  keeper_name     VARCHAR(20)  NOT NULL,
  keeper_tel      VARCHAR(15)  NOT NULL,
  keeper_location VARCHAR(50)  NOT NULL,
  PRIMARY KEY (keeper_id)
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------
-- 3. lost_items / 분실물
-- ---------------------------------------------------------------------
CREATE TABLE lost_items (
  lost_id          INT          NOT NULL AUTO_INCREMENT,
  user_primary_id  INT          NOT NULL,                 -- 등록한 회원 (members 연동)
  item_name        VARCHAR(50)  NOT NULL,
  category         VARCHAR(20)  NOT NULL,
  lost_date        DATE         NOT NULL,
  lost_location    VARCHAR(50)  NOT NULL,
  lost_status      TINYINT(1)   NOT NULL DEFAULT 0,       -- 0 = 찾는 중, 1 = 찾음
  lost_description TEXT         NOT NULL,
  created_at       DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at       DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  received_at      DATETIME     NULL,                     -- 수령 완료 시 기록 (그 전에는 NULL)
  PRIMARY KEY (lost_id),
  KEY idx_lost_user (user_primary_id),
  KEY idx_lost_name (item_name),
  KEY idx_lost_category (category),
  KEY idx_lost_date (lost_date),
  CONSTRAINT fk_lost_member
    FOREIGN KEY (user_primary_id) REFERENCES members (user_primary_id)
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------
-- 4. found_items / 습득물
-- ---------------------------------------------------------------------
CREATE TABLE found_items (
  found_id          INT          NOT NULL AUTO_INCREMENT,
  source_id         INT          NULL,                    -- 공공데이터 API 고유번호 (사용자 등록 습득물은 NULL)
  user_primary_id   INT          NULL,                    -- 등록한 회원 (공공데이터로 수집한 습득물은 NULL)
  item_name         VARCHAR(50)  NOT NULL,
  category          VARCHAR(20)  NOT NULL,
  found_date        DATE         NOT NULL,
  found_location    VARCHAR(50)  NOT NULL,
  keeper_id         VARCHAR(50)  NULL,                    -- 보관기관 (keepers 연동)
  found_status      TINYINT(1)   NOT NULL DEFAULT 0,      -- 0 = 찾는 중, 1 = 찾음
  found_description TEXT         NOT NULL,
  img_url           TEXT         NULL,
  created_at        DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at        DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (found_id),
  UNIQUE KEY uq_found_source (source_id),                 -- 같은 공공데이터를 중복 저장하지 않음
  KEY idx_found_user (user_primary_id),
  KEY idx_found_keeper (keeper_id),
  KEY idx_found_name (item_name),
  KEY idx_found_category (category),
  KEY idx_found_date (found_date),
  CONSTRAINT fk_found_member
    FOREIGN KEY (user_primary_id) REFERENCES members (user_primary_id) ON DELETE SET NULL,
  CONSTRAINT fk_found_keeper
    FOREIGN KEY (keeper_id) REFERENCES keepers (keeper_id) ON DELETE SET NULL
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------
-- (선택) C# 백엔드 전용 계정 - root 대신 사용 권장. 비밀번호를 바꾼 뒤 주석을 지우고 실행하세요.
-- ---------------------------------------------------------------------
-- CREATE USER 'lostfind_app'@'localhost' IDENTIFIED BY '여기에_비밀번호';
-- GRANT SELECT, INSERT, UPDATE, DELETE ON lostfind.* TO 'lostfind_app'@'localhost';
