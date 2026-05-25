-- MySQL schema generated from TrailMeisterDb code
-- Creates database, user, and tables expected by the application.

-- Adjust DB name, user and password to match AppSettings.Current if needed
CREATE DATABASE IF NOT EXISTS `skimeister` DEFAULT CHARACTER SET = utf8mb4 COLLATE = utf8mb4_unicode_ci;
USE `skimeister`;

-- Optional: create application user (adjust host/password as needed)
CREATE USER IF NOT EXISTS 'skimeister'@'localhost' IDENTIFIED BY 'P@ssw0rd';
GRANT ALL PRIVILEGES ON `skimeister`.* TO 'skimeister'@'localhost';
FLUSH PRIVILEGES;

-- Table: people
CREATE TABLE IF NOT EXISTS `people` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `firstName` VARCHAR(200) NOT NULL,
  `lastName` VARCHAR(200) DEFAULT '',
  `nickName` VARCHAR(200) DEFAULT '',
  `association` VARCHAR(200) DEFAULT '',
  PRIMARY KEY (`id`),
  INDEX `idx_people_firstName` (`firstName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table: tags
CREATE TABLE IF NOT EXISTS `tags` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `EPC` VARCHAR(255) NOT NULL,
  `PersonId` BIGINT UNSIGNED NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `ux_tags_epc` (`EPC`),
  KEY `idx_tags_personId` (`PersonId`),
  CONSTRAINT `fk_tags_person` FOREIGN KEY (`PersonId`) REFERENCES `people`(`id`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table: events
CREATE TABLE IF NOT EXISTS `events` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `EventName` VARCHAR(255) NOT NULL,
  `LapLength` INT NOT NULL,
  `EventDate` DATE NOT NULL,
  `EventFinished` TINYINT(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`),
  KEY `idx_events_date` (`EventDate`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table: laps
CREATE TABLE IF NOT EXISTS `laps` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `tagId` BIGINT UNSIGNED NOT NULL,
  `eventId` BIGINT UNSIGNED NOT NULL,
  `LapCount` INT UNSIGNED NOT NULL,
  `LapTime` BIGINT UNSIGNED NOT NULL,
  `TotalTime` BIGINT UNSIGNED NOT NULL,
  `PersonId` BIGINT UNSIGNED NULL,
  `LapLength` INT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_laps_tagId` (`tagId`),
  KEY `idx_laps_eventId` (`eventId`),
  KEY `idx_laps_personId` (`PersonId`),
  CONSTRAINT `fk_laps_tag` FOREIGN KEY (`tagId`) REFERENCES `tags`(`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_laps_event` FOREIGN KEY (`eventId`) REFERENCES `events`(`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_laps_person` FOREIGN KEY (`PersonId`) REFERENCES `people`(`id`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- End of schema
