mergeInto(LibraryManager.library, {
  NingoReportProgress: function (p) {
    try {
      window.dispatchEvent(new CustomEvent('ningo-scene-progress', { detail: p }));
    } catch (e) {}
  },
  NingoReportSceneDone: function () {
    try {
      window.dispatchEvent(new CustomEvent('ningo-scene-done'));
    } catch (e) {}
  }
});
