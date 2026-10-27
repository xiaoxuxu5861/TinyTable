using System;
using System.Collections.Generic;

namespace TinyTable
{
    public static partial class Localization
    {
        private static void InitializeLanguagePacks()
        {
            Languages.Add(new LanguageInfo("zh-CN", "简体中文"));
            Languages.Add(new LanguageInfo("zh-TW", "繁體中文"));
            Languages.Add(new LanguageInfo("en-US", "English"));
            Languages.Add(new LanguageInfo("de-DE", "Deutsch"));
            Languages.Add(new LanguageInfo("fr-FR", "Français"));
            Languages.Add(new LanguageInfo("es-ES", "Español"));
            Languages.Add(new LanguageInfo("it-IT", "Italiano"));
            Languages.Add(new LanguageInfo("pt-BR", "Português (Brasil)"));
            Languages.Add(new LanguageInfo("nl-NL", "Nederlands"));
            Languages.Add(new LanguageInfo("pl-PL", "Polski"));
            Languages.Add(new LanguageInfo("ru-RU", "Русский"));
            Languages.Add(new LanguageInfo("uk-UA", "Українська"));
            Languages.Add(new LanguageInfo("tr-TR", "Türkçe"));
            Languages.Add(new LanguageInfo("ja-JP", "日本語"));
            Languages.Add(new LanguageInfo("ko-KR", "한국어"));
            Languages.Add(new LanguageInfo("vi-VN", "Tiếng Việt"));
            Languages.Add(new LanguageInfo("id-ID", "Bahasa Indonesia"));
            Languages.Add(new LanguageInfo("cs-CZ", "Čeština"));

            string[] codes =
            {
                "de-DE", "fr-FR", "es-ES", "it-IT", "pt-BR",
                "nl-NL", "pl-PL", "ru-RU", "uk-UA", "tr-TR",
                "ja-JP", "ko-KR", "vi-VN", "id-ID", "cs-CZ"
            };
            foreach (string code in codes)
                ExtraTexts[code] = new Dictionary<string, string>(StringComparer.Ordinal);

            AddExtra(codes, "局部桌面备份  ·  只保护你选择的图标位置",
                "Selektive Desktop-Sicherung · Nur ausgewählte Symbolpositionen schützen",
                "Sauvegarde sélective du Bureau · Protéger les icônes choisies",
                "Copia selectiva del escritorio · Protege los iconos elegidos",
                "Backup selettivo del desktop · Proteggi le icone scelte",
                "Backup seletivo da área de trabalho · Proteja os ícones escolhidos",
                "Selectieve bureaubladback-up · Bescherm gekozen pictogrammen",
                "Selektywna kopia pulpitu · Chroń wybrane ikony",
                "Выборочная копия рабочего стола · Защита выбранных значков",
                "Вибіркова копія стільниці · Захист вибраних піктограм",
                "Seçmeli masaüstü yedeği · Seçilen simgeleri koru",
                "デスクトップの選択バックアップ · 選んだアイコン位置のみ保護",
                "선택적 바탕 화면 백업 · 선택한 아이콘 위치만 보호",
                "Sao lưu màn hình chọn lọc · Chỉ bảo vệ biểu tượng đã chọn",
                "Cadangan desktop selektif · Lindungi ikon yang dipilih",
                "Výběrová záloha plochy · Chránit vybrané ikony");

            AddExtra(codes, "刷新桌面", "Aktualisieren", "Actualiser", "Actualizar",
                "Aggiorna", "Atualizar", "Vernieuwen", "Odśwież", "Обновить",
                "Оновити", "Yenile", "更新", "새로 고침", "Làm mới", "Segarkan", "Obnovit");
            AddExtra(codes, "读取桌面选中项", "Auswahl", "Sélection",
                "Selección", "Selezione", "Seleção", "Selectie",
                "Zaznaczenie", "Выделение", "Виділене", "Seçim",
                "選択取得", "선택 가져오기", "Mục đã chọn", "Pilihan", "Výběr");
            AddExtra(codes, "选择方案", "Auswahlsatz", "Jeu de sélection", "Grupo",
                "Gruppo selezione", "Grupo", "Selectieset", "Zestaw wyboru",
                "Набор выбора", "Набір вибору", "Seçim kümesi", "選択セット", "선택 세트",
                "Bộ lựa chọn", "Set pilihan", "Sada výběru");
            AddExtra(codes, "新建局部快照", "Neuer selektiver Snapshot",
                "Nouvel instantané sélectif", "Nueva instantánea selectiva",
                "Nuova istantanea selettiva", "Novo instantâneo seletivo",
                "Nieuwe selectieve momentopname", "Nowa migawka wybiórcza",
                "Новый выборочный снимок", "Новий вибірковий знімок",
                "Yeni seçmeli anlık görüntü", "選択スナップショットを作成",
                "새 선택 스냅샷", "Ảnh chụp chọn lọc mới",
                "Snapshot selektif baru", "Nový výběrový snímek");
            AddExtra(codes, "保存", "Speichern", "Enregistrer", "Guardar", "Salva",
                "Salvar", "Opslaan", "Zapisz", "Сохранить", "Зберегти",
                "Kaydet", "保存", "저장", "Lưu", "Simpan", "Uložit");
            AddExtra(codes, "载入", "Laden", "Charger", "Cargar", "Carica", "Carregar",
                "Laden", "Wczytaj", "Загрузить", "Завантажити", "Yükle", "読込", "불러오기",
                "Tải", "Muat", "Načíst");
            AddExtra(codes, "保存方案", "Speichern", "Enregistrer", "Guardar", "Salva", "Salvar",
                "Opslaan", "Zapisz", "Сохранить", "Зберегти", "Kaydet", "保存", "저장",
                "Lưu", "Simpan", "Uložit");
            AddExtra(codes, "删除方案", "Löschen", "Supprimer", "Eliminar", "Elimina", "Excluir",
                "Verwijderen", "Usuń", "Удалить", "Видалити", "Sil", "削除", "삭제",
                "Xóa", "Hapus", "Odstranit");
            AddExtra(codes, "选择一个已保存方案…", "Gespeicherten Satz wählen…",
                "Choisir un jeu enregistré…", "Elegir un conjunto guardado…",
                "Scegli un gruppo salvato…", "Escolha um conjunto salvo…",
                "Kies een opgeslagen set…", "Wybierz zapisany zestaw…",
                "Выберите сохранённый набор…", "Виберіть збережений набір…",
                "Kayıtlı bir küme seçin…", "保存済みセットを選択…", "저장된 세트 선택…",
                "Chọn bộ đã lưu…", "Pilih set tersimpan…", "Vyberte uloženou sadu…");
            AddExtra(codes, "全选", "Alle", "Tout", "Todo", "Tutto", "Todos", "Alles",
                "Wszystko", "Все", "Усе", "Tümü", "すべて", "모두", "Tất cả", "Semua", "Vše");
            AddExtra(codes, "清空", "Leeren", "Effacer", "Limpiar", "Svuota", "Limpar", "Wissen",
                "Wyczyść", "Очистить", "Очистити", "Temizle", "クリア", "지우기", "Xóa hết", "Kosongkan", "Vymazat");
            AddExtra(codes, "快速选择 ▾", "Schnellauswahl ▾", "Sélection rapide ▾",
                "Selección rápida ▾", "Selezione rapida ▾", "Seleção rápida ▾",
                "Snel kiezen ▾", "Szybki wybór ▾", "Быстрый выбор ▾",
                "Швидкий вибір ▾", "Hızlı seçim ▾", "クイック選択 ▾",
                "빠른 선택 ▾", "Chọn nhanh ▾", "Pilih cepat ▾", "Rychlý výběr ▾");
            AddExtra(codes, "快速选择", "Schnellauswahl", "Sélection rapide",
                "Selección rápida", "Selezione rapida", "Seleção rápida",
                "Snel kiezen", "Szybki wybór", "Быстрый выбор",
                "Швидкий вибір", "Hızlı seçim", "クイック選択",
                "빠른 선택", "Chọn nhanh", "Pilih cepat", "Rychlý výběr");
            AddExtra(codes, "选择一个或多个项目类型",
                "Einen oder mehrere Elementtypen wählen",
                "Choisissez un ou plusieurs types d’éléments",
                "Elige uno o varios tipos de elementos",
                "Scegli uno o più tipi di elementi",
                "Escolha um ou mais tipos de item",
                "Kies een of meer itemtypen",
                "Wybierz jeden lub więcej typów elementów",
                "Выберите один или несколько типов элементов",
                "Виберіть один або кілька типів елементів",
                "Bir veya daha fazla öğe türü seçin",
                "1つ以上の項目タイプを選択",
                "하나 이상의 항목 유형 선택",
                "Chọn một hoặc nhiều loại mục",
                "Pilih satu atau beberapa jenis item",
                "Vyberte jeden nebo více typů položek");
            AddExtra(codes, "应用选择", "Auswahl anwenden", "Appliquer la sélection",
                "Aplicar selección", "Applica selezione", "Aplicar seleção",
                "Selectie toepassen", "Zastosuj wybór", "Применить выбор",
                "Застосувати вибір", "Seçimi uygula", "選択を適用",
                "선택 적용", "Áp dụng lựa chọn", "Terapkan pilihan", "Použít výběr");
            AddExtra(codes, "种类型", "Typen", "types", "tipos", "tipi", "tipos",
                "typen", "typów", "типов", "типів", "tür", "種類", "개 유형",
                "loại", "jenis", "typů");
            AddExtra(codes, "更多 ▾", "Mehr ▾", "Plus ▾", "Más ▾", "Altro ▾",
                "Mais ▾", "Meer ▾", "Więcej ▾", "Ещё ▾", "Більше ▾",
                "Daha fazla ▾", "その他 ▾", "더 보기 ▾", "Thêm ▾", "Lainnya ▾", "Více ▾");
            AddExtra(codes, "快照操作 ▾", "Snapshot ▾", "Instantané ▾", "Instantánea ▾",
                "Istantanea ▾", "Instantâneo ▾", "Momentopname ▾", "Migawka ▾",
                "Снимок ▾", "Знімок ▾", "Anlık görüntü ▾", "スナップショット ▾",
                "스냅샷 ▾", "Ảnh chụp ▾", "Snapshot ▾", "Snímek ▾");
            AddExtra(codes, "备份勾选项目", "Snapshot speichern", "Enregistrer l'instantané",
                "Guardar instantánea", "Salva istantanea", "Salvar instantâneo",
                "Momentopname opslaan", "Zapisz migawkę", "Сохранить снимок", "Зберегти знімок",
                "Anlık görüntüyü kaydet", "スナップショット保存", "스냅샷 저장", "Lưu ảnh chụp",
                "Simpan snapshot", "Uložit snímek");
            AddExtra(codes, "刷新右键菜单", "Kontextmenü", "Menu contextuel", "Menú contextual",
                "Menu contestuale", "Menu de contexto", "Contextmenu", "Menu kontekstowe",
                "Контекстное меню", "Контекстне меню", "Bağlam menüsü", "右クリックメニュー",
                "상황에 맞는 메뉴", "Menu ngữ cảnh", "Menu konteks", "Místní nabídka");
            AddExtra(codes, "恢复", "Wiederherstellen", "Restaurer", "Restaurar", "Ripristina",
                "Restaurar", "Herstellen", "Przywróć", "Восстановить", "Відновити", "Geri yükle",
                "復元", "복원", "Khôi phục", "Pulihkan", "Obnovit");
            AddExtra(codes, "删除快照", "Snapshot löschen", "Supprimer l'instantané",
                "Eliminar instantánea", "Elimina istantanea", "Excluir instantâneo",
                "Momentopname verwijderen", "Usuń migawkę", "Удалить снимок", "Видалити знімок",
                "Anlık görüntüyü sil", "スナップショット削除", "스냅샷 삭제", "Xóa ảnh chụp",
                "Hapus snapshot", "Odstranit snímek");
            AddExtra(codes, "撤销恢复", "Wiederherstellung rückgängig", "Annuler la restauration",
                "Deshacer restauración", "Annulla ripristino", "Desfazer restauração",
                "Herstel ongedaan maken", "Cofnij przywracanie", "Отменить восстановление",
                "Скасувати відновлення", "Geri yüklemeyi geri al", "復元を元に戻す",
                "복원 실행 취소", "Hoàn tác khôi phục", "Urungkan pemulihan", "Vrátit obnovení");
            AddExtra(codes, "设置", "Einstellungen", "Paramètres", "Ajustes", "Impostazioni",
                "Configurações", "Instellingen", "Ustawienia", "Настройки", "Налаштування",
                "Ayarlar", "設定", "설정", "Cài đặt", "Pengaturan", "Nastavení");

            AddExtra(codes, "备份", "Sichern", "Sauver", "Guardar", "Backup", "Backup", "Back-up",
                "Kopia", "Копия", "Копія", "Yedek", "保護", "백업", "Sao lưu", "Cadangan", "Záloha");
            AddExtra(codes, "桌面项目", "Desktop-Element", "Élément du Bureau", "Elemento del escritorio",
                "Elemento desktop", "Item da área de trabalho", "Bureaubladitem", "Element pulpitu",
                "Элемент рабочего стола", "Елемент стільниці", "Masaüstü öğesi", "デスクトップ項目",
                "바탕 화면 항목", "Mục màn hình", "Item desktop", "Položka plochy");
            AddExtra(codes, "类型", "Typ", "Type", "Tipo", "Tipo", "Tipo", "Type", "Typ", "Тип",
                "Тип", "Tür", "種類", "유형", "Loại", "Jenis", "Typ");
            AddExtra(codes, "当前位置", "Aktuelle Position", "Position actuelle", "Posición actual",
                "Posizione attuale", "Posição atual", "Huidige positie", "Bieżąca pozycja",
                "Текущая позиция", "Поточна позиція", "Geçerli konum", "現在位置", "현재 위치",
                "Vị trí hiện tại", "Posisi saat ini", "Aktuální pozice");
            AddExtra(codes, "来源", "Quelle", "Source", "Origen", "Origine", "Origem", "Bron", "Źródło",
                "Источник", "Джерело", "Kaynak", "場所", "원본", "Nguồn", "Sumber", "Zdroj");

            AddExtra(codes, "文件夹", "Ordner", "Dossiers", "Carpetas", "Cartelle", "Pastas", "Mappen",
                "Foldery", "Папки", "Папки", "Klasörler", "フォルダー", "폴더", "Thư mục", "Folder", "Složky");
            AddExtra(codes, "快捷方式", "Verknüpfungen", "Raccourcis", "Accesos directos", "Collegamenti",
                "Atalhos", "Snelkoppelingen", "Skróty", "Ярлыки", "Ярлики", "Kısayollar", "ショートカット",
                "바로 가기", "Lối tắt", "Pintasan", "Zástupci");
            AddExtra(codes, "图片", "Bilder", "Images", "Imágenes", "Immagini", "Imagens", "Afbeeldingen",
                "Obrazy", "Изображения", "Зображення", "Resimler", "画像", "이미지", "Hình ảnh", "Gambar", "Obrázky");
            AddExtra(codes, "文档", "Dokumente", "Documents", "Documentos", "Documenti", "Documentos",
                "Documenten", "Dokumenty", "Документы", "Документи", "Belgeler", "文書", "문서", "Tài liệu", "Dokumen", "Dokumenty");
            AddExtra(codes, "压缩包", "Archive", "Archives", "Archivos", "Archivi", "Arquivos", "Archieven",
                "Archiwa", "Архивы", "Архіви", "Arşivler", "アーカイブ", "압축 파일", "Tệp nén", "Arsip", "Archivy");
            AddExtra(codes, "音视频", "Audio & Video", "Audio et vidéo", "Audio y vídeo", "Audio e video",
                "Áudio e vídeo", "Audio en video", "Audio i wideo", "Аудио и видео", "Аудіо й відео",
                "Ses ve video", "音声と動画", "오디오 및 비디오", "Âm thanh & video", "Audio & video", "Zvuk a video");
            AddExtra(codes, "其他", "Andere", "Autres", "Otros", "Altro", "Outros", "Overig", "Inne",
                "Прочее", "Інше", "Diğer", "その他", "기타", "Khác", "Lainnya", "Ostatní");
            AddExtra(codes, "系统图标", "Systemsymbole", "Icônes système", "Iconos del sistema",
                "Icone di sistema", "Ícones do sistema", "Systeempictogrammen", "Ikony systemowe",
                "Системные значки", "Системні піктограми", "Sistem simgeleri", "システムアイコン",
                "시스템 아이콘", "Biểu tượng hệ thống", "Ikon sistem", "Systémové ikony");

            AddExtra(codes, "取消", "Abbrechen", "Annuler", "Cancelar", "Annulla", "Cancelar", "Annuleren",
                "Anuluj", "Отмена", "Скасувати", "İptal", "キャンセル", "취소", "Hủy", "Batal", "Zrušit");
            AddExtra(codes, "保存设置", "Einstellungen speichern", "Enregistrer", "Guardar ajustes",
                "Salva impostazioni", "Salvar configurações", "Instellingen opslaan", "Zapisz ustawienia",
                "Сохранить настройки", "Зберегти налаштування", "Ayarları kaydet", "設定を保存",
                "설정 저장", "Lưu cài đặt", "Simpan pengaturan", "Uložit nastavení");
            AddExtra(codes, "常驻与启动", "Tray & Start", "Zone de notification", "Bandeja e inicio",
                "Tray e avvio", "Bandeja e inicialização", "Systeemvak & opstart", "Zasobnik i start",
                "Трей и запуск", "Трей і запуск", "Tepsi ve başlangıç", "トレイと起動", "트레이 및 시작",
                "Khay & khởi động", "Baki & mulai", "Oznamovací oblast a spuštění");
            AddExtra(codes, "自动保存", "Automatisch speichern", "Sauvegarde auto", "Guardado automático",
                "Salvataggio automatico", "Salvamento automático", "Automatisch opslaan", "Autozapis",
                "Автосохранение", "Автозбереження", "Otomatik kaydet", "自動保存", "자동 저장",
                "Tự động lưu", "Simpan otomatis", "Automatické ukládání");
            AddExtra(codes, "恢复与命名", "Wiederherstellung & Namen", "Restauration et noms",
                "Restauración y nombres", "Ripristino e nomi", "Restauração e nomes", "Herstel en namen",
                "Przywracanie i nazwy", "Восстановление и имена", "Відновлення й назви",
                "Geri yükleme ve adlar", "復元と名前", "복원 및 이름", "Khôi phục & đặt tên",
                "Pulihkan & nama", "Obnovení a názvy");
            AddExtra(codes, "界面语言", "Sprache", "Langue", "Idioma", "Lingua", "Idioma", "Taal", "Język",
                "Язык", "Мова", "Dil", "表示言語", "인터페이스 언어", "Ngôn ngữ", "Bahasa", "Jazyk");
            AddExtra(codes, "浏览器选择上传文件时临时收纳桌面快捷方式",
                "Desktop-Verknüpfungen während der Browser-Dateiauswahl vorübergehend ausblenden",
                "Masquer temporairement les raccourcis du Bureau pendant la sélection de fichiers",
                "Guardar temporalmente los accesos directos durante la selección de archivos",
                "Nascondi temporaneamente i collegamenti durante la selezione dei file",
                "Ocultar temporariamente os atalhos durante a seleção de arquivos",
                "Bureaubladsnelkoppelingen tijdelijk opbergen tijdens bestandsselectie",
                "Tymczasowo ukrywaj skróty pulpitu podczas wyboru pliku",
                "Временно скрывать ярлыки рабочего стола при выборе файла",
                "Тимчасово приховувати ярлики стільниці під час вибору файла",
                "Dosya seçerken masaüstü kısayollarını geçici olarak kaldır",
                "ブラウザーのファイル選択中はデスクトップのショートカットを一時退避",
                "브라우저 파일 선택 중 바탕 화면 바로 가기 임시 보관",
                "Tạm ẩn lối tắt màn hình khi chọn tệp trong trình duyệt",
                "Simpan sementara pintasan desktop saat memilih berkas",
                "Dočasně skrýt zástupce plochy při výběru souboru");
            AddExtra(codes, "实验：需关闭自动排列并保持 TinyTable 运行；选择窗口关闭后自动归还。",
                "Experimentell: Auto-Anordnen ausschalten und TinyTable geöffnet lassen.",
                "Expérimental : désactivez le classement automatique et laissez TinyTable actif.",
                "Experimental: desactiva la organización automática y mantén TinyTable abierto.",
                "Sperimentale: disattiva la disposizione automatica e mantieni TinyTable attivo.",
                "Experimental: desative a organização automática e mantenha o TinyTable aberto.",
                "Experimenteel: schakel automatisch schikken uit en laat TinyTable actief.",
                "Eksperymentalne: wyłącz auto-rozmieszczanie i pozostaw TinyTable uruchomiony.",
                "Экспериментально: отключите автоупорядочивание и не закрывайте TinyTable.",
                "Експериментально: вимкніть автоупорядкування й не закривайте TinyTable.",
                "Deneysel: otomatik düzenlemeyi kapatın ve TinyTable'ı çalışır tutun.",
                "試験機能：自動整列を無効にし、TinyTable を起動したままにしてください。",
                "실험 기능: 자동 정렬을 끄고 TinyTable을 계속 실행하세요.",
                "Thử nghiệm: tắt tự sắp xếp và giữ TinyTable đang chạy.",
                "Eksperimental: matikan susun otomatis dan biarkan TinyTable berjalan.",
                "Experimentální: vypněte automatické řazení a nechte TinyTable spuštěný.");
            AddExtra(codes, "打开 TinyTable", "TinyTable öffnen", "Ouvrir TinyTable", "Abrir TinyTable",
                "Apri TinyTable", "Abrir TinyTable", "TinyTable openen", "Otwórz TinyTable",
                "Открыть TinyTable", "Відкрити TinyTable", "TinyTable'ı aç", "TinyTable を開く",
                "TinyTable 열기", "Mở TinyTable", "Buka TinyTable", "Otevřít TinyTable");
            AddExtra(codes, "退出 TinyTable", "TinyTable beenden", "Quitter TinyTable", "Salir de TinyTable",
                "Esci da TinyTable", "Sair do TinyTable", "TinyTable afsluiten", "Zamknij TinyTable",
                "Выйти из TinyTable", "Вийти з TinyTable", "TinyTable'dan çık", "TinyTable を終了",
                "TinyTable 종료", "Thoát TinyTable", "Keluar TinyTable", "Ukončit TinyTable");
            AddExtra(codes, "加入 TinyTable 备份列表", "Zur TinyTable-Sicherung hinzufügen",
                "Ajouter à la liste TinyTable", "Añadir a la lista de TinyTable", "Aggiungi all'elenco TinyTable",
                "Adicionar à lista do TinyTable", "Aan TinyTable-lijst toevoegen", "Dodaj do listy TinyTable",
                "Добавить в список TinyTable", "Додати до списку TinyTable", "TinyTable listesine ekle",
                "TinyTable リストに追加", "TinyTable 목록에 추가", "Thêm vào danh sách TinyTable",
                "Tambahkan ke daftar TinyTable", "Přidat do seznamu TinyTable");
            AddExtra(codes, "从 TinyTable 备份列表移除", "Aus TinyTable-Sicherung entfernen",
                "Retirer de la liste TinyTable", "Quitar de la lista de TinyTable", "Rimuovi dall'elenco TinyTable",
                "Remover da lista do TinyTable", "Uit TinyTable-lijst verwijderen", "Usuń z listy TinyTable",
                "Удалить из списка TinyTable", "Видалити зі списку TinyTable", "TinyTable listesinden kaldır",
                "TinyTable リストから削除", "TinyTable 목록에서 제거", "Xóa khỏi danh sách TinyTable",
                "Hapus dari daftar TinyTable", "Odebrat ze seznamu TinyTable");
        }

        private static void AddExtra(
            string[] codes, string chinese, params string[] translations)
        {
            if (translations.Length != codes.Length)
                throw new InvalidOperationException(
                    "Language pack entry count mismatch: " + chinese);
            for (int i = 0; i < codes.Length; i++)
                ExtraTexts[codes[i]][chinese] = translations[i];
        }
    }
}
