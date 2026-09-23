/** Yönetim paneli betiği: liste satırı ekle/sil ve SMTP alanlarını göster/gizle. */
(function () {
  'use strict';

  /* Liste satırı ekleme ve silme -------------------------------------- */
  document.addEventListener('click', function (olay) {
    var ekle = olay.target.closest('[data-ekle]');
    if (ekle) {
      var liste = ekle.closest('[data-liste]');
      var sablon = liste && liste.querySelector('[data-sablon]');
      if (sablon) {
        var kopya = sablon.content.cloneNode(true);
        sablon.parentNode.insertBefore(kopya, sablon);
        var eklenen = sablon.previousElementSibling;
        var ilk = eklenen && eklenen.querySelector('input, textarea, select');
        if (ilk) ilk.focus();
      }
      return;
    }

    var sil = olay.target.closest('[data-sil]');
    if (sil) {
      var satir = sil.closest('[data-satir]');
      if (satir) satir.remove();
    }
  });

  /* SMTP alanlarını yöntem seçimine göre göster ----------------------- */
  var secici = document.querySelector('[data-smtp-secici]');
  var alanlar = document.getElementById('smtp-alanlari');
  if (secici && alanlar) {
    secici.addEventListener('change', function () {
      alanlar.hidden = secici.value !== 'smtp';
    });
  }

  /* Kaydedilmemiş değişiklikle sayfadan ayrılmayı uyar ----------------- */
  var form = document.querySelector('form[method="post"]');
  var degisti = false;
  if (form) {
    form.addEventListener('input', function () {
      degisti = true;
    });
    form.addEventListener('submit', function () {
      degisti = false;
    });
    window.addEventListener('beforeunload', function (olay) {
      if (degisti) {
        olay.preventDefault();
        olay.returnValue = '';
      }
    });
  }
})();
