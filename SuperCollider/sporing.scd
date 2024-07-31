(
/*
Place the cursor here and hit Ctrl+Enter

If you want to stop the audio, hit Ctrl+.

If you stop the audio, you need to exit the game. Hit Ctrl+Enter (to start the audio file), and then start the game again.
*/

var s, bpm, beat, currentPath, resourcesPath, waterFilePath, replaceSlash;

// Define server
s = Server.default;

// Define groups
~mainThemeGroup = Group.new;
~padsGroup = Group.new(~mainThemeGroup);
~pianoGroup = Group.new(~mainThemeGroup);
~kalimbaGroup = Group.new(~mainThemeGroup);
~drumsGroup = Group.new(~mainThemeGroup);
~bassGroup = Group.new(~mainThemeGroup);
~pulseGroup = Group.new(~mainThemeGroup);

// Define global amplitude variables
~ampPads = 1;
~ampPiano = 0;
~ampDrums = 0.0055;
~ampBass = 0;
~ampPulse = 0;
~waterValue1 = 1.0;
~waterValue2 = 1.0;


// Replace slash function
replaceSlash = { |path|
    path.asString.replace("/", "\\\\")
};

// Resources path
currentPath = thisProcess.nowExecutingPath;
resourcesPath = PathName(currentPath).pathOnly +/+ "resources";

// ---------- Main Theme ----------
// Pads
SynthDef(\apadMH, { arg
	out = 0, pan = 0, freq = 880, amp = 0.2, att = 0.4, dec = 0.5, sus = 0.26, rel = 1.0, gate = 1,
	vibratoRate = 4, vibratoDepth = 0.015, tremoloRate = 0.5,
	//From 0 to 1:
	tremoloDepth = 0.65, reverbMix = 0.5, roomSize = 1, damp = 0.5;

	var env, snd, vibrato, tremolo, mod2, mod3;

	env = Env.adsr(att, dec, sus, rel).kr(gate: gate);
	vibrato = SinOsc.kr(vibratoRate).range(freq * (1 - vibratoDepth), freq * (1 + vibratoDepth));
	tremolo = LFNoise2.kr(1).range(0.2, 1) * SinOsc.kr(tremoloRate).range((1 - tremoloDepth), 1);

	snd = SinOsc.ar(freq: [freq, vibrato], mul:(env * tremolo * amp)).distort;
	snd = Mix.ar([snd]);
	snd = FreeVerb.ar(snd, reverbMix, roomSize, damp);

	DetectSilence.ar(snd, 0.0001, 0.05, doneAction: 2);
	Out.ar(out, LinPan2.ar(snd, pan));
},
metadata:(
	credit: "Mike Hairston",
)
).add;

// Piano
SynthDef(\cheapPiano1, {
	arg
	out = 0, freq = 440, amp = 0.1, att = 0.02, rel = 10, pan = 0,
	//From 0 to 1
	tone = 0.1, hollowness = 0.3;

	var hammer, snd, delay, tonefreq, env;

	env = Env.perc(att, rel, amp * 4, -1).ar(doneAction: 2);

	delay = (1 / (freq * [2.pow(-0.04/12), 1, 2.pow(0.03/12)]));
	tonefreq = tone.linlin(0, 1, 1000, 5000);

	hammer = Decay2.ar(
		in: Impulse.ar(0.001),
		attackTime: 0.008,
		decayTime: 0.14,
		mul: LFNoise2.ar(freq: amp.linlin(0, 1, tonefreq, 2 * tonefreq), mul: 0.25)
	);

	snd = CombL.ar(hammer, delay, delay, 50 * amp);
	snd = HPF.ar(snd, hollowness.linlin(0, 1, 50, 1000));
	snd = snd * env;

	snd = Limiter.ar(snd);

	snd = Limiter.ar(snd);
	Out.ar(out, Pan2.ar(snd, pan));
},
metadata: (
	credit: "based on something posted 2008-06-17 by jeff, based on an old example by james mcc",
)
).add;


// Drums
SynthDef(\abstractDrum, {
	arg
	out = 0, freq = 140, amp = 1, pan = 0, att = 0.001, dec = 0.01, rel = 1,
	//From 0 to 1
	position = 0.5, ampSlope = 3, decCoef = 0.15, hiFreqSus = 0;

	var freqarray, amparray, decarray, exciter, snd;

	freqarray = Array.fill(8, {
		arg i = 1;
		(
			Array.fill((i + 1), {
				arg j;
				(j + 1).pow(2)
			}) +
			(i + 1).pow(2)
		).sqrt

	});
	freqarray = freqarray.flatten/(2.sqrt);

	amparray = Array.fill(36, {
		arg i;
		if (freqarray[i] > 20000)
		{ 0 }
		{
			sin(((i + 1) * pi) * position) *
			(ampSlope * (freqarray[i]).log2).dbamp
		}
	});
	amparray = amparray/ampSlope;

	decarray = Array.fill(36, {
		arg i;
		exp(-1 * i * decCoef) + hiFreqSus
	});
	decarray = decarray/decarray[0];

	exciter = Decay2.ar(
		in: Impulse.ar(0),
		attackTime: att,
		decayTime: dec,
		mul: 0.005);

	snd = Klank.ar(
		specificationsArrayRef:
		Ref.new([freqarray, amparray, decarray]),
		input: exciter,
		freqscale: freq,
		decayscale: rel);

	snd = Mix.ar(snd * amp * 2);
	snd = Limiter.ar(snd);

	DetectSilence.ar(in: snd, doneAction: 2);

	Out.ar(out, Pan2.ar(snd, pan));
},
metadata: (
	credit: "by Josh Mitchell",
)
).add;


// Bass
SynthDef(\subBass2, {
	arg
	//Blend goes from 0 to 1
	out = 0, amp = 0.4, pan = 0, sus = 1, freq = 440, att = 4, rel = 7, curve = 1.5, blend = 1;

	var env, in, ina, synca, octa, inb, syncb, octb, octave, snd;

	env = Env.linen(att, sus, rel, amp, [curve, -1 * curve]).kr(doneAction: 2);

	in = LFPar.ar(freq: freq * 2, iphase: 1);
	ina = LFPar.ar(freq: freq * 2, iphase: 1);
	inb = LFPar.ar(freq: freq * 2, iphase: -1);

	synca = LFPulse.ar(freq: freq, iphase: 0);
	syncb = LFPulse.ar(freq: freq, iphase: 0.5);

	octa = ina * synca;
	octb = inb * syncb;
	octave = Mix.ar([octa, octb]);

	snd = Mix.ar([octave * blend, in * (blend - 1)]);
	snd = LeakDC.ar(snd);
	snd = Limiter.ar(in: snd, level: 1);
	snd = snd * env;
	snd = LPF.ar(snd, 3000);
	// snd = FreeVerb.ar(snd, mix: 0.35, room: 05, damp: 1);

	Out.ar(out, Pan2.ar(snd, pan));
},
metadata: (
	credit: "Josh Mitchell, 2019. Slightly modified by edomedo.",
)
).add;


// Pulse
SynthDef("pulseRisset", {arg freq = 440, att = 0.01, rel = 11, amp = 0.1, pan = 0, pulseFreq = 8;
	var partials, durs, amps, snd, env;
	partials = [246.4, 247.4, 404.8, 406.5, 523.6, 748, 880, 1206, 1320, 1654, 1791];
	partials = (partials / 440) * freq;
	durs = [11, 10, 7, 6, 4, 3.4, 3, 2.2, 2, 1.1, 1] / 11;
	amps = durs.linlin(1, 11, 0.2, 1);

	env = Env.perc(
		attackTime: att,
		releaseTime: durs * rel,
		level: amps
	).kr(doneAction: [2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]);

	snd = Pulse.ar(partials) * env * amp / 11;

	snd = RLPF.ar(
		in: snd,
		freq: freq * LFPulse.ar(pulseFreq).range(2, 4),
		rq: SinOsc.ar(LFNoise2.kr(1).range(4, 9)).range(0.1, 0.3));

	snd = LPF.ar(snd, 12000);
	snd = Limiter.ar(Mix.ar(snd));

	Out.ar(0, Pan2.ar(snd,pan) );
},
metadata: (
	credit: "by Bruno Tucunduva Ruviaro, based on Jean-Claude Risset's bell",
)
).add;

// ---------- Effects ----------
// ----- SynthDefs -----

// Water effect
waterFilePath = (currentPath.dirname +/+ "/"  +/+ "resources/water.mp3");
waterFilePath = replaceSlash.(waterFilePath);

w = Buffer.read(s, waterFilePath);

SynthDef(
    \sfgrain, {
        arg bufnum=0, pan=0.0, startPos=0.0, amp=0.1, dur=0.04, globalamp=0;
        var grain;

        grain = PlayBuf.ar(2, bufnum, BufRateScale.kr(bufnum), 1, BufFrames.ir(bufnum) * startPos, 0) * (EnvGen.kr(Env.perc(0.01, dur), doneAction: 2) - 0.001);

        Out.ar(0, Pan2.ar(grain, pan) * globalamp);
    }
).add;


// Walk
SynthDef(\walkDirt, {
	|out = 0, amp = 0.28, atk = 0.035, sus = 0.5, rel = 0.12|
	var env, pinkNoise, lowNoise, combinedNoise, filter,
	randAmp, randFreq, dust, granulatedNoise;

	randAmp = amp * (1 + Rand(-0.70, -0.75));
	randFreq = LFNoise1.kr(1).range(1150, 1250);
	pinkNoise = PinkNoise.ar(randAmp);
	lowNoise = LPF.ar(WhiteNoise.ar(randAmp * 0.5), 4000);
	combinedNoise = pinkNoise + lowNoise;
	env = EnvGen.kr(Env.perc(atk, rel, sus), doneAction: 2);
	combinedNoise = combinedNoise * env;
	filter = BPF.ar(combinedNoise, randFreq, 0.3);
	dust = Dust.ar(20000);
	granulatedNoise = filter * dust;

	Out.ar(out, (granulatedNoise * 12)!2);
}).add;

// Wind
SynthDef(\wind, {
	| out = 0, freq = 500, mRatio = 1, cRatio = 1, index = 1, iScale = 3,
	atk = 0.01, rel = 4, cAtk = 4, cRel = (-2), pan = 0, amp = 0.2 |

	var car, mod, mod2, env, iEnv;

	iEnv = EnvGen.kr(
		Env(
			[index, index * iScale, index],
			[atk, rel],
			[cAtk, cRel]
		)
	);
	env = EnvGen.kr(
		Env.perc(atk, rel, curve: [cAtk, cRel]),
		doneAction: 2
	);
	mod2 = SinOsc.ar(freq/10, mul: freq/10 * iEnv);
	mod = SinOsc.ar(freq * mRatio + mod2, mul: freq * mRatio * iEnv);
	car = SinOsc.ar(freq * cRatio + mod) * env * amp;
	car = Pan2.ar(car, pan);
	Out.ar(out, car);
}).add;

// Voice 1
SynthDef(\voice1, {
	arg
	out = 0, freq = 440, gate = 1, pan = 0, amp = 0.1, att = 0.001, rel = 1, lfoSpeed = 4.8, inputLevel = 0.2,
	// From 0 to 1
	modIndex = 0.2, mix = 0.2, lfoDepth = 0.1;

	var env1, env2, env3, env4;
	var osc1, osc2, osc3, osc4, snd;

	env1 = Env.perc(att, rel * 1.25, inputLevel, curve: \lin).kr;
	env2 = Env.perc(att, rel, inputLevel, curve: \lin).kr;
	env3 = Env.perc(att, rel * 1.5, inputLevel, curve: \lin).kr;
	env4 = Env.perc(att, rel * 1.5, inputLevel, curve: \lin).kr;

	osc4 = SinOsc.ar(freq) * 6.7341546494171 * modIndex * env4;
	osc3 = SinOsc.ar(freq * 2, osc4) * env3;
	osc2 = SinOsc.ar(freq * 30) * 0.683729941 * env2;
	osc1 = SinOsc.ar(freq * 2, osc2) * env1;
	snd = Mix((osc3 * (1 - mix)) + (osc1 * mix));
	snd = snd * (SinOsc.ar(lfoSpeed).range((1 - lfoDepth), 1));

	snd = snd * Env.asr(0, 1, 0.1).kr(gate: gate, doneAction: 2);
	snd = Pan2.ar(snd, pan, amp);

	Out.ar(out, snd);
},
metadata: (
	credit: "Nathan Ho",
)
).add;

// Voice 2
SynthDef(\voice2, {
	arg
	out = 0, pan = 0, freq = 100, amp = 1, rel = 1.5,
	impulseAtt = 0.5, impulseSus = 3, impulseDec = 4, impulseHold = 1,
	// From 0 to 1.
	filtermin = 250, filtermax = 5000, rq = 0.65, thresh = 0.4, ratio = 2.5;

	var total, exciter, snd;

	total = (impulseAtt + impulseSus + impulseDec + impulseHold) * freq;

	exciter = Env.new(
		levels: [0, 1, 1, 0, 0],
		times: [impulseAtt, impulseSus, impulseDec, impulseHold]/total).ar;

	snd = CombN.ar(
		in: exciter,
		maxdelaytime: 0.06,
		delaytime: 1/freq,
		decaytime: rel);

	snd = RLPF.ar(
		in: snd,
		freq: LinExp.ar(Amplitude.ar(in: snd), 0, 1, filtermin, filtermax),
		rq: rq);

	snd = CompanderD.ar(
		in: snd,
		thresh: thresh,
		slopeBelow: 1,
		slopeAbove: 1/ratio);

	snd = Mix.ar(snd) * amp;
	snd = Limiter.ar(snd);

	DetectSilence.ar(in: snd, doneAction: 2);

	Out.ar(out, Pan2.ar(snd, pan));

},
metadata: (
	credit: "Josh Mitchell",
)
).add;

SynthDef(\jump, { |transpose, amp = 0.2|
	var snd, env;

	env = Env.perc(0.01, 0.3, curve: 'sine' ).ar;

	snd = SinOsc.ar(
		freq: EnvGen.kr(Env([400, 700, 1000] + transpose, [0.02, 0.05 ]), doneAction: 2),
		mul: env
	);

	snd = snd + (WhiteNoise.ar(0.02) * env);

	snd = snd * EnvGen.kr(Env([1, 0], [0.07], curve: 'lin'));

	snd = snd * amp;
	Out.ar(0, snd ! 2);
}).add;


// --------- Execution ---------
OSCFunc({
	// Main theme
Routine({

	Pbindef(\pads,
		\instrument, "apadMH",
		\group, ~padsGroup,
		\dur, 16,
		\amp,  Pfunc { 0.5 * ~ampPads },
		\ctranspose, -10,
		\strum, Pseq([Pwhite(0.95, 1, inf)], inf),
		\note, Pseq([
			Pcollect({ |chord| chord.scramble }, Pseq([
				[0, 16, 7, 11, 26], // Cmaj9
				[16, 7, 11, 26, 18], // Em11
				[7, 23, 14, 18, 21], // G11
				[9, 17, 12, 28, 19] // Fmaj9
			], inf))
		], inf),
		\pan, Pseq([Pwhite(-1, 1, inf)], inf)
	).play;

	Pbindef(\drums,
		\instrument, "abstractDrum",
		\group, ~drumsGroup,
		\amp, Pseq(
			[0.7 * Pfunc { ~ampDrums }, 0.7 * Pfunc { ~ampDrums }, 0.8 * Pfunc { ~ampDrums },
				1 * Pfunc { ~ampDrums }, 0.8 * Pfunc { ~ampDrums }, 0.8 * Pfunc { ~ampDrums },
				0.8* Pfunc { ~ampDrums }, 0.7 * Pfunc { ~ampDrums }, 0.7 * Pfunc { ~ampDrums },
				0.8 * Pfunc { ~ampDrums }, 0.8* Pfunc { ~ampDrums }, 0.8* Pfunc { ~ampDrums },
				0.8 * Pfunc { ~ampDrums }] * 1.8 , inf),
		\strum, Pwhite(-0.1, 0.1),
		\freq, Pseq(
			[   50,
				1700,
				233,
				Pwrand([400, 600, 800], [3, 1, 1].normalizeSum, 1),
				233,
				Pwrand([400, 600, 800], [1, 3, 1].normalizeSum, 1),
				Pwrand([400, 600, 800], [1, 1, 3].normalizeSum, 1),
				50,
				1700,
				233,
				Pwrand([400, 600, 800], [1, 1, 3].normalizeSum, 1),
				Pwrand([400, 600, 800], [1, 3, 1].normalizeSum, 1),
				233
		], inf),
		\dur, Pseq(
			[1, 1, 3, 1, 0.5, 0.5, 1,
				1, 1, 3, 0.5, 0.5, 2
		] * 1, inf),
		\position, Pseq(
			[0.4, 0.7, 0.2, 0.4, 0.2, 0.3, 0.3,
				0.4, 0.7, 0.2, 0.3, 0.3, 0.2
		], inf ),
		\pan, Pseq(
			[0, 0, 0, -0.6, 0, -0.2, 0.6,
				0, 0, 0, 0.6, -0.6, 0
		], inf),
		\decCoef, Pseq(
			[0.3, 0.01, 0.05, 1, 0.05, 1, 1,
				0.3, 0.1, 0.05, 1, 1, 0.05
		], inf),
		\hiFreqSus, Pseq(
			[0.05, 0.6, 0.4, 0, 0.4, 0, 0,
				0.05, 0.6, 0.4, 0, 0, 0.4
		], inf),
		\rel, Pseq(
			[1.5, 4, 0.6, 3, 0.6, 3, 3,
				1.5, 3.5, 0.6, 3, 3, 0.6
		], inf),
	).play;

	Pbindef(\bass,
		\instrument, "subBass2",
		\group, ~bassGroup,
		\note, Pseq([
			Pshuf([2, -6], 1),   // Cmaj9
			Pshuf([4, -1], 1),  // Em11
			Pshuf([11, -3], 1), // G11
			Pshuf([9, 4], 1)    // Fmaj9
		], inf),
		\dur, Pseq([Pseq([10, 6], 1), Pseq([6, 10], 1)], inf),
		\legato, Pwhite(0.7, 0.75, inf),
		\sus, 0.3,
		\ctranspose, -24,
		\blend, Pwhite(0, -0.5, inf),
		\amp, 0.25 * Pfunc { ~ampBass }
	).play;


	Pbindef(\pulse,
		\instrument, "pulseRisset",
		\group, ~pulseGroup,
		\freq, Prand([261.63, 329.63, 329.63, 392.00, 392.00, 440.00] * 2, inf),
		\dur, Pseq([Pwhite(12, 20)], inf),
		\rel, Pseq([Pwhite(5, 11)], inf),
		\amp, Pwhite(0.05, 0.15) * Pfunc { ~ampPulse },
		\pan, Pseq([Pwhite(-1, 1)], inf),
		\pulseFreq, Pseq([Pwhite(4, 9)], inf)
	).play;

	9.wait;

	Pbindef(\piano,
		\instrument, \cheapPiano1,
		\group, ~pianoGroup,
		\ctranspose, -10,
		\legato, Pseq([0.9, 0.5], inf),
		\dur, Pseq([16], inf),
		\strum, Pseq([Pwhite(0.5, 0.6, inf)], inf),
		\note, Pseq([
			Pcollect({ |chord| chord.scramble }, Pseq([
				[0, 4, 7, 11],   // Cmaj9
				[4, 7, 11, 14],  // Em11
				[7, 11, 14, 18], // G11
				[-3, 5, 0, 9]    // Fmaj9
			], inf))
		], inf),
		\amp, (Pwhite(0.12, 0.17) * 0.68) * Pfunc { ~ampPiano },
		\pan, Pseq([Pwhite(-1, 1)], inf)
	).play;

}).play;

}, '/start');

// OSC functions
OSCFunc({
	~ampPads = 1;
}, '/ampPads');

OSCFunc({
	~ampPiano = 1;
}, '/ampPiano');

OSCFunc({
	~ampPulse = 1;
}, '/ampPulse');

OSCFunc({
	~ampBass = 1;
}, '/ampBass');

OSCFunc({
	~ampDrums = 1;
}, '/ampDrums');

// Fx
OSCFunc({ arg msg;
	var receivedValuew = msg;
	~waterValue1 = receivedValuew[1];
}, '/waterProximity1');

OSCFunc({ arg msg;
	var receivedValuew = msg;
	~waterValue2 = receivedValuew[1];
}, '/waterProximity2');

OSCFunc({
	Synth(\walkDirt);

}, '/walk');

// Voices
// translator
OSCFunc({ arg msg;
	var receivedValue, scale, n, noteArrayPattern, noteArray, durPattern;
	scale = [0, 2, 4, 5, 7, 9]; // cMajor
	n = 6;
	noteArrayPattern = Prand(scale, n);
	noteArray = noteArrayPattern.asStream.nextN(n);
	durPattern = Pshuf([Pwhite(0.2, 0.4, n + 1)], inf);
	// receivedValue = msg[1];

	Pbindef(\translatorTalk,
		\instrument, "voice1",
		\ctranspose, 10,
		\dur, durPattern,
		\note, Pseq(noteArray, 1),
		\mix, 0.1,
		\modIndex, Pseq([Pwhite(0.1,0.3)],inf),
		\lfoSpeed, 0.8,
		\lfoDepth, 0.9,
		\inputLevel, Pgauss(0.8, 0.1, inf),
		\amp, 0.065,
		\strum, Pwhite(0, 0.02)
	).play;
}, '/translatorTalk');

OSCFunc({ arg msg;
	Pbind(
		\instrument, "voice1",
		\ctranspose, 12,
		\dur, Pseq([0.2, 0.3]),
		\note, Pseq([2, 0]),
		\mix, 0.1,
		\modIndex, Pseq([Pwhite(0.1,0.3)],inf),
		\lfoSpeed, 0.8,
		\lfoDepth, 0.9,
		\inputLevel, Pgauss(0.8, 0.1, inf),
		\amp, 0.065,
	).play;
}, '/translatorNo');


// interactor
OSCFunc({ arg msg;
	var receivedValue, scale, n, noteArrayPattern, noteArray, durPattern;
	scale = [0, 2, 3, 5, 7, 9, 10]; // eDorian
	n = 6;
	noteArrayPattern = Prand(scale, n);
	noteArray = noteArrayPattern.asStream.nextN(n);
	durPattern = Pshuf([Pwhite(0.1, 0.4, n + 1)], inf);
	// receivedValue = msg[1];

	Pbindef(\interactorTalk,
		\instrument, "voice1",
		\ctranspose, -12,
		\dur, durPattern,
		\note, Pseq(noteArray, 1),
		\mix, 0.1,
		\modIndex, Pseq([Pwhite(0.1,0.3)],inf),
		\lfoSpeed, 0.8,
		\lfoDepth, 0.9,
		\inputLevel, Pgauss(0.8, 0.1, inf),
		\amp, 0.16,
		\strum, Pwhite(0, 0.02)
	).play;
}, '/interactorTalk');

OSCFunc({ arg msg;
	Pbind(
		\instrument, "voice1",
		\ctranspose, -12,
		\dur, Pseq([0.2, 0.3]),
		\note, Pseq([2, 0]),
		\mix, 0.1,
		\modIndex, Pseq([Pwhite(0.1,0.3)],inf),
		\lfoSpeed, 0.8,
		\lfoDepth, 0.9,
		\inputLevel, Pgauss(0.8, 0.1, inf),
		\amp, 0.16,
	).play;
}, '/interactorNo');

// trampoline
OSCFunc({ arg msg;
	var receivedValue, scale, n, noteArrayPattern, noteArray, durPattern;
	scale = [0, 2, 4, 5, 7, 9, 10]; // gMixolydian
	n = 6;
	noteArrayPattern = Prand(scale, n);
	noteArray = noteArrayPattern.asStream.nextN(n);
	durPattern = Pshuf([Pwhite(0.1, 0.6, n + 1)], inf);
	// receivedValue = msg[1];

	Pbindef(\trampolineTalk,
		\instrument, "voice2",
		\ctranspose, 0,
		\dur, durPattern,
		\note, Pseq(noteArray, 1),
		\mix, 0.1,
		\modIndex, Pseq([Pwhite(0.1,0.5)],inf),
		\lfoSpeed, 0.8,
		\lfoDepth, 0.9,
		\inputLevel, Pgauss(0.8, 0.1, inf),
		\amp, 0.23,
		\strum, Pwhite(0, 0.02)
	).play;
}, '/trampolineTalk');

OSCFunc({ arg msg;
	Pbind(
		\instrument, "voice2",
		\dur, Pseq([0.2, 0.3]),
		\ctranspose, 0,
		\note, Pseq([2,0]),
		\amp, 0.23,
		\rel, Pseq([0.3, 0.3]),
		\impulseAtt, Pseq([1,1]),
		\impulseSus, Pseq([1,1]),
		\impulseDec, Pseq([2,1]),
		\impulseHold, Pseq([0.3,0.1]),
	).play;
}, '/trampolineNo');


// platform
OSCFunc({ arg msg;
	var receivedValue, scale, n, noteArrayPattern, noteArray, durPattern;
	scale = [0, 2, 4, 5, 7, 9, 11]; // fMajor
	n = 6;
	noteArrayPattern = Prand(scale, n);
	noteArray = noteArrayPattern.asStream.nextN(n);
	durPattern = Pshuf([Pwhite(0.1, 0.6, n + 1)], inf);
	// receivedValue = msg[1];

	Pbindef(\platformTalk,
		\instrument, "voice2",
		\ctranspose, -24,
		\dur, durPattern,
		\note, Pseq(noteArray, 1),
		\mix, 0.1,
		\modIndex, Pseq([Pwhite(0.1,0.3)],inf),
		\lfoSpeed, 0.8,
		\lfoDepth, 0.9,
		\inputLevel, Pgauss(0.7, 0.1, inf),
		\amp, 0.33,
		\strum, Pwhite(0, 0.02)
	).play;

}, '/platformTalk');

OSCFunc({ arg msg;
	Pbind(
		\instrument, "voice2",
		\dur, Pseq([0.2, 0.3]),
		\ctranspose, -24,
		\note, Pseq([2,0]),
		\amp, 0.33,
		\rel, Pseq([0.3, 0.3]),
		\impulseAtt, Pseq([1,1]),
		\impulseSus, Pseq([1,1]),
		\impulseDec, Pseq([2,1]),
		\impulseHold, Pseq([0.3,0.1]),
	).play;
}, '/platformNo');

OSCFunc({ arg msg;
	var transpose = msg[1], jumps = msg[2];
	Pbind(
		\instrument, \jump,
		\freq, Pkey(\note).midicps,
		\note, Pseq([0], jumps),
		\dur, Pwhite(0.35, 0.4),
		\transpose, transpose,
		\amp, 0.151
	).play;
}, '/fungiJump');


// Water init
{
	inf.do{ arg i;
		var prop, timestart, timeend, amplitude;
		prop = (i % 300) / 300;
		timestart = prop * 0.8;
		timeend = prop * (0.8 + (0.1 * ~waterValue1));

		amplitude = (1 - ~waterValue1);

		Synth(\sfgrain, [
			\bufnum, w.bufnum,
			\startPos, rrand(timestart, timeend),
			\amp, exprand(0.005, 0.1),
			\globalamp, amplitude * 0.06,
			\pan, ~waterValue1.rand2,
			\dur, 0.1 + (~waterValue1 * 0.5)
		]);

		(((~waterValue1 * 0.15) + 0.01).max(0.01)).wait;
	};
}.fork;

{
	inf.do{ arg i;
		var prop, timestart, timeend, amplitude;
		prop = (i % 300) / 300;
		timestart = prop * 0.8;
		timeend = prop * (0.8 + (0.1 * ~waterValue2));

		amplitude = (1 - ~waterValue2);

		Synth(\sfgrain, [
			\bufnum, w.bufnum,
			\startPos, rrand(timestart, timeend),
			\amp, exprand(0.005, 0.1),
			\globalamp, amplitude * 0.06,
			\pan, ~waterValue2.rand2,
			\dur, 0.1 + (~waterValue2 * 0.5)
		]);

		(((~waterValue2 * 0.15) + 0.01).max(0.01)).wait;
	};
}.fork;

// Wind init
{
	inf.do{ arg i;
		var waittime = rrand(45, 100);
		waittime.wait;
		Synth(\wind, [
			\freq, 50.midicps,
			\index, 8,
			\iScale, 3.5,
			\cRatio, 2,
			\mRatio, 1,
			\amp, 0.012,
			\atk, 4,
			\rel, 8,
		]);
	};
}.fork;


)
s.scope;