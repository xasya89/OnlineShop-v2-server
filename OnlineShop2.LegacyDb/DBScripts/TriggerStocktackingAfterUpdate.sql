CREATE TRIGGER `shop7`.`stocktakings_AFTER_UPDATE` AFTER UPDATE ON `stocktakings` FOR EACH ROW
BEGIN
	IF OLD.Status<>2 AND NEW.Status=2 THEN
		INSERT INTO documenthistories (DocumentId, DocumentType, Status, Processed) VALUES (NEW.id, 6, 0, 0);
	END IF;
END
