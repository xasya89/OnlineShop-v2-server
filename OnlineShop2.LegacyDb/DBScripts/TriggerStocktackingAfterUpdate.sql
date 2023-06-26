/*0 - �������� ����� */
CREATE TRIGGER shifts_AFTER_INSERT AFTER INSERT ON shifts FOR EACH ROW BEGIN
	INSERT INTO documenthistories (DocumentId, DocumentType, Status) VALUES (NEW.id, 0, 0);
END

//1 - �������� �����
CREATE TRIGGER `shop7`.`stocktakings_AFTER_UPDATE` AFTER UPDATE ON `stocktakings` FOR EACH ROW
BEGIN
	IF OLD.Status<>2 AND NEW.Status=2 THEN
		INSERT INTO documenthistories (DocumentId, DocumentType, Status, Processed) VALUES (NEW.id, 6, 0, 0);
	END IF;
END

//2 - ����� ���
CREATE TRIGGER `checksells_AFTER_INSERT` AFTER INSERT ON `checksells` FOR EACH ROW BEGIN
	INSERT INTO documenthistories (DocumentId, DocumentType, Status) VALUES (NEW.id, 2, 0);
END



//6 - ���������� ��������������
CREATE TRIGGER `stocktakings_AFTER_UPDATE` AFTER UPDATE ON `stocktakings` FOR EACH ROW BEGIN
	IF OLD.Status<>2 AND NEW.Status=2 THEN
		INSERT INTO documenthistories (DocumentId, DocumentType, Status, Processed) VALUES (NEW.id, 6, 0, 0);
	END IF;
END

//3 - ����� �����
CREATE TRIGGER `shop7`.`goods_AFTER_INSERT` AFTER INSERT ON `goods` FOR EACH ROW
BEGIN
	INSERT INTO documenthistories (DocumentId, DocumentType, Status) VALUES (NEW.id, 3, 0);
END

//4 - ���������� �����
CREATE TRIGGER `goods_AFTER_UPDATE` AFTER UPDATE ON `goods` FOR EACH ROW BEGIN
	INSERT INTO documenthistories (DocumentId, DocumentType, Status) VALUES (NEW.id, 4, 0);
END