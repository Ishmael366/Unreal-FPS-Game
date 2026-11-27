mergeInto(LibraryManager.library, {
 StudyWebStatus: function(p){if(window.studyUI)window.studyUI.receive(JSON.parse(UTF8ToString(p)));},
 StudyNetHost: function(p){window.StudyNetwork.host(UTF8ToString(p));},
 StudyNetJoin: function(p,q){window.StudyNetwork.join(UTF8ToString(p),UTF8ToString(q));},
 StudyNetAnswer: function(p){window.StudyNetwork.answer(UTF8ToString(p));},
 StudyNetInput: function(x,z){window.StudyNetwork.input(x,z);},
 StudyNetDispose: function(){if(window.StudyNetwork)window.StudyNetwork.close();}
});
