using WCell.RealmServer.AI;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Events.Asda2
{
    internal class DefenceTownEventAplia : DeffenceTownEvent
    {
        public DefenceTownEventAplia(Map map, int minLevel, int maxLevel, float amountMod, float healthMod,
            float otherStatsMod, float speedMod, float difficulty)
            : base(map, minLevel, maxLevel, amountMod, healthMod, otherStatsMod, speedMod, difficulty)
        {
        }

        protected override void InitMovingPaths()
        {
            this.AddMovingPath(this._map).Add(88, 260).Add(89, 261).Add(90, 263).Add(91, 263).Add(91, 264).Add(92, 266)
                .Add(93, 267).Add(94, 268).Add(94, 269).Add(94, 270).Add(95, 271).Add(96, 273).Add(97, 273).Add(97, 274)
                .Add(98, 276).Add(99, 277).Add(99, 278).Add(100, 278).Add(100, 279).Add(100, 280).Add(101, 281)
                .Add(101, 282).Add(102, 283).Add(103, 284).Add(105, 286).Add(105, 287).Add(105, 288).Add(106, 289)
                .Add(106, 291).Add(107, 293).Add(107, 294).Add(107, 295).Add(107, 296).Add(107, 297).Add(107, 298)
                .Add(107, 299).Add(108, 300).Add(108, 301).Add(108, 302).Add(108, 303).Add(108, 304).Add(108, 305)
                .Add(108, 306).Add(108, 307).Add(108, 308).Add(108, 309).Add(108, 310).Add(109, 311).Add(109, 312)
                .Add(109, 313).Add(109, 314).Add(109, 315).Add(109, 316).Add(110, 317).Add(110, 318).Add(110, 319)
                .Add(110, 320).Add(110, 321).Add(110, 322).Add(110, 323).Add(110, 324).Add(110, 325).Add(110, 326)
                .Add(111, 327).Add(112, 328).Add(112, 329).Add(112, 330).Add(112, 331).Add(112, 332).Add(112, 333)
                .Add(112, 334).Add(112, 335).Add(112, 336).Add(112, 337).Add(113, 338).Add(113, 339).Add(113, 340)
                .Add(113, 341).Add(113, 342).Add(113, 343).Add(113, 344).Add(113, 345).Add(113, 346).Add(113, 347)
                .Add(113, 348).Add(113, 349).Add(113, 350).Add(114, 351).Add(114, 352).Add(114, 353).Add(114, 354)
                .Add(114, 355).Add(114, 356).Add(114, 357).Add(114, 358).Add(114, 359).Add(114, 360).Add(114, 361)
                .Add(114, 362).Add(114, 363).Add(114, 364).Add(114, 365).Add(114, 366).Add(114, 367).Add(114, 368)
                .Add(114, 369).Add(114, 370).Add(114, 371).Add(114, 372).Add(114, 373).Add(114, 374).Add(114, 375)
                .Add(114, 376).Add(114, 377).Add(114, 378).Add(114, 379).Add(113, 380).Add(112, 381).Add(111, 382)
                .Add(111, 384).Add(110, 385).Add(109, 385).Add(108, 385).Add(107, 385).Add(106, 385).Add(105, 385)
                .Add(104, 385).Add(103, 385).Add(102, 385).Add(101, 385).Add(102, 386).Add(102, 387).Add(102, 388)
                .Add(102, 389).Add(102, 390).Add(103, 387).Add(103, 388).Add(103, 389).Add(103, 390).Add(103, 391)
                .Add(103, 392).Add(103, 393).Add(103, 394).Add(103, 395).Add(104, 396).Add(104, 397).Add(104, 398)
                .Add(104, 399).Add(104, 400).Add(104, 401).Add(105, 402).Add(105, 403).Add(105, 404).Add(105, 405)
                .Add(105, 406).Add(105, 407).Add(105, 408).Add(105, 409).Add(106, 410).Add(106, 411).Add(106, 412)
                .Add(106, 413).Add(106, 414).Add(107, 415).Add(108, 416).Add(109, 417).Add(110, 418).Add(110, 419)
                .Add(110, 420).Add(110, 421).Add(111, 422).Add(112, 423).Add(112, 424).Add(112, 425).Add(112, 426)
                .Add(113, 427).Add(113, 428).Add(114, 429).Add(114, 430).Add(114, 431).Add(114, 432).Add(115, 433)
                .Add(116, 434).Add(116, 434).Add(116, 435).Add(117, 436).Add(117, 438).Add(117, 439).Add(117, 440)
                .Add(117, 441).Add(117, 442).Add(117, 443).Add(117, 444).Add(117, 445).Add(117, 446).Add(117, 447)
                .Add(117, 448).Add(117, 449).Add(117, 450).Add(117, 451).Add(117, 452).Add(117, 453).Add(117, 454)
                .Add(116, 456).Add(115, 456).Add(114, 456).Add(113, 456).Add(112, 456).Add(111, 456).Add(110, 456)
                .Add(109, 456).Add(108, 456).Add(107, 456).Add(106, 456).Add(105, 456).Add(104, 456).Add(103, 456)
                .Add(102, 456).Add(101, 456).Add(100, 456).Add(99, 456).Add(98, 456).Add(97, 456).Add(96, 456)
                .Add(95, 456).Add(94, 456).Add(93, 456).Add(92, 456).Add(91, 456).Add(90, 456).Add(89, 456).Add(88, 456)
                .Add(87, 456);
            this.AddMovingPath(this._map).Add(213, 396).Add(211, 396).Add(210, 396).Add(209, 396).Add(208, 396)
                .Add(206, 396).Add(205, 396).Add(204, 396).Add(203, 396).Add(202, 397).Add(201, 397).Add(200, 397)
                .Add(199, 397).Add(197, 396).Add(196, 397).Add(195, 397).Add(194, 397).Add(193, 397).Add(192, 398)
                .Add(190, 398).Add(189, 399).Add(188, 399).Add(187, 399).Add(186, 399).Add(184, 399).Add(183, 399)
                .Add(182, 399).Add(181, 399).Add(180, 399).Add(179, 399).Add(178, 399).Add(177, 399).Add(175, 399)
                .Add(174, 400).Add(172, 401).Add(171, 401).Add(170, 401).Add(169, 401).Add(167, 401).Add(166, 401)
                .Add(165, 400).Add(163, 400).Add(162, 400).Add(161, 400).Add(160, 400).Add(159, 400).Add(157, 400)
                .Add(156, 400).Add(155, 399).Add(154, 399).Add(153, 399).Add(152, 399).Add(150, 399).Add(148, 399)
                .Add(146, 399).Add(145, 399).Add(143, 398).Add(142, 400).Add(141, 401).Add(140, 401).Add(139, 402)
                .Add(138, 403).Add(137, 404).Add(136, 404).Add(134, 405).Add(133, 406).Add(132, 407).Add(131, 408)
                .Add(129, 409).Add(128, 410).Add((int) sbyte.MaxValue, 410).Add(126, 410).Add(125, 411).Add(123, 412)
                .Add(122, 412).Add(120, 413).Add(119, 413).Add(118, 413).Add(116, 412).Add(115, 412).Add(113, 411)
                .Add(112, 411).Add(111, 411).Add(109, 410).Add(108, 410).Add(107, 408).Add(106, 407).Add(105, 406)
                .Add(104, 406).Add(103, 405).Add(102, 404).Add(101, 402).Add(101, 400).Add(100, 399).Add(100, 398)
                .Add(100, 397).Add(100, 396).Add(99, 394).Add(99, 393).Add(99, 392).Add(98, 390).Add(98, 389)
                .Add(98, 388).Add(98, 387).Add(98, 385).Add(97, 384).Add(97, 382).Add(96, 381).Add(96, 380).Add(95, 379)
                .Add(94, 378).Add(93, 376).Add(92, 375).Add(91, 374).Add(90, 373).Add(89, 372).Add(88, 371).Add(87, 370)
                .Add(86, 369).Add(85, 368).Add(84, 367).Add(83, 366).Add(82, 365).Add(80, 364).Add(79, 363).Add(78, 361)
                .Add(77, 361).Add(86, 360).Add(74, 359).Add(73, 357).Add(72, 356).Add(71, 354).Add(69, 354).Add(67, 354)
                .Add(63, 353).Add(64, 352).Add(63, 351).Add(63, 349).Add(62, 348).Add(61, 347).Add(61, 346).Add(61, 345)
                .Add(62, 344).Add(64, 343).Add(65, 343).Add(68, 342).Add(70, 343).Add(70, 345);
            this.AddMovingPath(this._map).Add(70, 346).Add(70, 348).Add(70, 349).Add(70, 351).Add(70, 353).Add(71, 354)
                .Add(71, 355).Add(72, 356).Add(73, 357).Add(74, 358).Add(75, 360).Add(75, 361).Add(76, 362).Add(78, 364)
                .Add(80, 365).Add(81, 366).Add(82, 367).Add(83, 368).Add(84, 369).Add(85, 370).Add(87, 371).Add(88, 372)
                .Add(88, 372).Add(89, 373).Add(90, 374).Add(92, 375).Add(93, 377).Add(94, 379).Add(96, 380).Add(97, 382)
                .Add(97, 384).Add(97, 385).Add(96, 387).Add(96, 388).Add(96, 389).Add(96, 391).Add(96, 392).Add(96, 394)
                .Add(96, 395).Add(96, 396).Add(96, 398).Add(96, 400).Add(98, 402).Add(99, 404).Add(99, 405)
                .Add(100, 406).Add(101, 408).Add(102, 410).Add(102, 411).Add(103, 412).Add(103, 413).Add(104, 415)
                .Add(104, 416).Add(104, 417).Add(104, 418).Add(106, 420).Add(108, 422).Add(109, 424).Add(109, 426)
                .Add(110, 427).Add(110, 429).Add(110, 430).Add(111, 432).Add(111, 433).Add(111, 434).Add(111, 436)
                .Add(111, 438).Add(111, 440).Add(111, 441).Add(111, 443).Add(112, 444).Add(112, 446).Add(112, 448)
                .Add(112, 449).Add(112, 451).Add(112, 453).Add(112, 455).Add(112, 457).Add(112, 458).Add(112, 460)
                .Add(112, 462).Add(113, 464).Add(113, 466).Add(70, 346).Add(113, 468).Add(113, 470).Add(113, 472)
                .Add(113, 473);
            this.AddMovingPath(this._map).Add(212, 401).Add(211, 401).Add(209, 401).Add(207, 401).Add(206, 401)
                .Add(204, 401).Add(203, 401).Add(201, 400).Add(199, 401).Add(197, 401).Add(196, 401).Add(194, 402)
                .Add(193, 402).Add(192, 403).Add(190, 403).Add(188, 403).Add(187, 403).Add(185, 403).Add(183, 403)
                .Add(184, 404).Add(183, 404).Add(181, 404).Add(179, 404).Add(178, 404).Add(176, 404).Add(174, 404)
                .Add(172, 404).Add(170, 404).Add(168, 404).Add(166, 404).Add(164, 404).Add(162, 404).Add(160, 404)
                .Add(158, 404).Add(157, 404).Add(156, 403).Add(155, 402).Add(153, 401).Add(151, 400).Add(149, 399)
                .Add(148, 398).Add(145, 398).Add(144, 396).Add(142, 395).Add(141, 394).Add(139, 392).Add(137, 391)
                .Add(136, 390).Add(136, 389).Add(134, 388).Add(133, 387).Add(132, 386).Add(131, 385).Add(130, 384)
                .Add(129, 383).Add((int) sbyte.MaxValue, 382).Add(125, 382).Add(123, 381).Add(122, 381).Add(120, 380)
                .Add(118, 380).Add(116, 379).Add(114, 379).Add(112, 378).Add(110, 378).Add(108, 378).Add(106, 377)
                .Add(105, 377).Add(103, 376).Add(101, 376).Add(100, 375).Add(99, 375).Add(97, 374).Add(95, 374)
                .Add(93, 374).Add(92, 373).Add(90, 373).Add(89, 372).Add(87, 370).Add(86, 369).Add(85, 368).Add(84, 368)
                .Add(83, 367).Add(81, 365).Add(80, 364).Add(79, 363).Add(77, 362).Add(76, 361).Add(75, 359).Add(74, 358)
                .Add(73, 356).Add(72, 354).Add(70, 353).Add(69, 352).Add(67, 352).Add(66, 351).Add(64, 350).Add(62, 350)
                .Add(61, 349).Add(60, 347).Add(60, 345).Add(60, 344).Add(61, 343).Add(62, 342).Add(63, 342).Add(65, 341)
                .Add(66, 341).Add(68, 340).Add(69, 340).Add(70, 340).Add(72, 341).Add(72, 343).Add(73, 344);
            this.AddMovingPath(this._map).Add(72, 345).Add(72, 346).Add(72, 348).Add(72, 350).Add(73, 352).Add(73, 354)
                .Add(74, 356).Add(75, 358).Add(75, 359).Add(76, 361).Add(77, 363).Add(78, 364).Add(80, 365).Add(81, 365)
                .Add(82, 366).Add(83, 367).Add(84, 368).Add(85, 369).Add(86, 370).Add(88, 372).Add(89, 372).Add(90, 374)
                .Add(91, 375).Add(91, 376).Add(92, 378).Add(93, 379).Add(93, 381).Add(94, 383).Add(94, 384).Add(94, 386)
                .Add(94, 388).Add(94, 389).Add(94, 391).Add(95, 393).Add(95, 394).Add(95, 395).Add(95, 396).Add(96, 397)
                .Add(96, 399).Add(97, 400).Add(98, 402).Add(98, 403).Add(99, 405).Add(99, 406).Add(100, 407)
                .Add(100, 408).Add(101, 409).Add(102, 411).Add(102, 412).Add(103, 413).Add(103, 415).Add(103, 416)
                .Add(104, 418).Add(104, 420).Add(105, 422).Add(106, 423).Add(106, 425).Add(107, 427).Add(107, 428)
                .Add(107, 430).Add(108, 431).Add(108, 433).Add(108, 435).Add(108, 436).Add(108, 438).Add(108, 439)
                .Add(108, 441).Add(108, 443).Add(108, 444).Add(108, 446).Add(108, 448).Add(108, 449).Add(108, 450)
                .Add(109, 452).Add(109, 453).Add(109, 455).Add(109, 457).Add(109, 459).Add(109, 461).Add(109, 463)
                .Add(110, 465);
            this.AddMovingPath(this._map).Add(94, 263).Add(95, 264).Add(96, 265).Add(97, 266).Add(98, 267).Add(98, 268)
                .Add(99, 270).Add(100, 271).Add(101, 272).Add(102, 273).Add(103, 275).Add(104, 277).Add(105, 278)
                .Add(106, 279).Add(107, 280).Add(108, 282).Add(108, 283).Add(109, 285).Add(109, 286).Add(109, 287)
                .Add(110, 289).Add(110, 290).Add(111, 292).Add(111, 293).Add(111, 294).Add(111, 295).Add(112, 297)
                .Add(112, 298).Add(112, 299).Add(112, 301).Add(112, 302).Add(112, 304).Add(112, 306).Add(112, 307)
                .Add(112, 309).Add(112, 310).Add(112, 312).Add(112, 313).Add(112, 315).Add(112, 316).Add(112, 318)
                .Add(112, 320).Add(112, 321).Add(112, 323).Add(112, 325).Add(112, 326).Add(112, 328).Add(112, 330)
                .Add(112, 332).Add(112, 334).Add(112, 335).Add(112, 337).Add(112, 339).Add(112, 341).Add(112, 343)
                .Add(112, 344).Add(112, 346).Add(112, 348).Add(112, 349).Add(112, 351).Add(112, 353).Add(112, 355)
                .Add(112, 357).Add(112, 359).Add(112, 361).Add(113, 363).Add(114, 365).Add(114, 366).Add(115, 367)
                .Add(115, 368).Add(116, 369).Add(117, 370).Add(119, 371).Add(120, 373).Add(121, 374).Add(122, 375)
                .Add(123, 376).Add(124, 377).Add(125, 378).Add(126, 379).Add(128, 380).Add(129, 382).Add(131, 384)
                .Add(132, 385).Add(133, 387).Add(133, 389).Add(133, 390);
            this.AddMovingPath(this._map).Add(88, 260).Add(89, 261).Add(90, 263).Add(91, 263).Add(91, 264).Add(92, 266)
                .Add(93, 267).Add(94, 268).Add(94, 269).Add(94, 270).Add(95, 271).Add(96, 273).Add(97, 273).Add(97, 274)
                .Add(98, 276).Add(99, 277).Add(99, 278).Add(100, 278).Add(100, 279).Add(100, 280).Add(101, 281)
                .Add(101, 282).Add(102, 283).Add(103, 284).Add(105, 286).Add(105, 287).Add(105, 288).Add(106, 289)
                .Add(106, 291).Add(107, 293).Add(107, 294).Add(107, 295).Add(107, 296).Add(107, 298).Add(107, 299)
                .Add(108, 300).Add(108, 301).Add(108, 302).Add(108, 303).Add(108, 304).Add(108, 305).Add(108, 306)
                .Add(108, 307).Add(108, 308).Add(108, 309).Add(108, 310).Add(109, 311).Add(109, 312).Add(109, 313)
                .Add(109, 314).Add(109, 315).Add(109, 316).Add(110, 317).Add(110, 318).Add(110, 319).Add(110, 320)
                .Add(110, 321).Add(110, 322).Add(110, 323).Add(110, 324).Add(110, 325).Add(110, 326).Add(111, 327)
                .Add(112, 328).Add(112, 329).Add(112, 330).Add(112, 331).Add(112, 332).Add(112, 333).Add(112, 334)
                .Add(112, 335).Add(112, 336).Add(112, 337).Add(113, 338).Add(113, 339).Add(113, 340).Add(113, 341)
                .Add(113, 342).Add(113, 343).Add(113, 344).Add(113, 345).Add(113, 346).Add(113, 347).Add(113, 348)
                .Add(113, 349).Add(113, 350).Add(114, 351).Add(114, 352).Add(114, 353).Add(114, 354).Add(114, 355)
                .Add(114, 356).Add(114, 357).Add(114, 358).Add(114, 359).Add(114, 360).Add(114, 361).Add(114, 362)
                .Add(114, 361).Add(114, 362).Add(114, 363).Add(114, 364).Add(114, 365).Add(114, 366).Add(114, 367)
                .Add(114, 368).Add(114, 369).Add(114, 370).Add(114, 371).Add(114, 372).Add(114, 373).Add(114, 374)
                .Add(114, 375).Add(114, 376).Add(114, 377).Add(114, 378).Add(114, 379).Add(113, 380).Add(112, 381)
                .Add(112, 382).Add(111, 383).Add(111, 384).Add(110, 385).Add(109, 385).Add(108, 385).Add(107, 385)
                .Add(106, 385).Add(105, 385).Add(104, 385).Add(103, 385).Add(102, 385).Add(101, 385).Add(102, 386)
                .Add(102, 387).Add(102, 388).Add(102, 389).Add(102, 390).Add(103, 391).Add(103, 392).Add(103, 393)
                .Add(103, 394).Add(103, 395).Add(104, 396).Add(104, 397).Add(104, 398).Add(104, 399).Add(104, 400)
                .Add(104, 401).Add(105, 402).Add(105, 403).Add(105, 404).Add(105, 405).Add(105, 406).Add(105, 407)
                .Add(105, 408).Add(105, 409).Add(106, 410).Add(106, 411).Add(106, 412).Add(106, 413).Add(106, 414)
                .Add(107, 415).Add(108, 416).Add(109, 417).Add(110, 418).Add(110, 419).Add(110, 420).Add(110, 421)
                .Add(111, 422).Add(112, 423).Add(112, 424).Add(112, 425).Add(112, 426).Add(113, 427).Add(113, 428)
                .Add(114, 429).Add(114, 430).Add(114, 431).Add(114, 432).Add(115, 433).Add(116, 434).Add(116, 435)
                .Add(117, 436).Add(117, 437).Add(117, 438).Add(117, 439).Add(117, 440).Add(117, 441).Add(117, 442)
                .Add(117, 443).Add(117, 444).Add(117, 445).Add(117, 446).Add(117, 447).Add(117, 448).Add(117, 449)
                .Add(117, 450).Add(117, 451).Add(117, 452).Add(117, 453).Add(117, 454).Add(116, 456).Add(115, 456)
                .Add(114, 456).Add(113, 456).Add(112, 456).Add(111, 456).Add(110, 456).Add(109, 456).Add(108, 456)
                .Add(107, 456).Add(106, 456).Add(105, 456).Add(104, 456).Add(103, 456).Add(102, 456).Add(101, 456)
                .Add(100, 456).Add(99, 456).Add(98, 456).Add(97, 456).Add(96, 456).Add(95, 456).Add(94, 456)
                .Add(93, 456).Add(92, 456).Add(91, 456).Add(90, 456).Add(89, 456).Add(88, 456).Add(87, 456);
            this.AddMovingPath(this._map).Add(134, 391).Add(134, 393).Add(134, 395).Add(135, 397).Add(135, 398)
                .Add(134, 400).Add(134, 401).Add(133, 403).Add(132, 405).Add(131, 406).Add(130, 408).Add(129, 409)
                .Add((int) sbyte.MaxValue, 410).Add(126, 411).Add(124, 412).Add(123, 412).Add(121, 413).Add(120, 413)
                .Add(118, 414).Add(116, 415).Add(115, 415).Add(113, 416).Add(111, 417).Add(109, 418).Add(108, 418)
                .Add(107, 419).Add(105, 419).Add(104, 420).Add(103, 421).Add(102, 422).Add(100, 422).Add(99, 423)
                .Add(97, 423).Add(96, 424).Add(94, 424).Add(92, 424).Add(90, 425).Add(88, 425).Add(86, 426).Add(85, 427)
                .Add(83, 427).Add(82, 428).Add(80, 428).Add(78, 429).Add(77, 429).Add(75, 430).Add(73, 430).Add(71, 431)
                .Add(69, 431).Add(67, 432).Add(66, 433).Add(65, 433).Add(64, 434).Add(63, 434).Add(62, 435).Add(61, 435)
                .Add(60, 436).Add(58, 437).Add(57, 437);
            base.InitMovingPaths();
        }

        protected override int ExpPortionsTotal
        {
            get { return (int) (200.0 * (double) this._difficulty); }
        }

        protected override int EventItemsTotal
        {
            get { return (int) (100.0 * (double) this._difficulty); }
        }

        protected override void InitMonsterSpawn(float amountMod)
        {
            this.AddSpawnEntry(NpcCustomEntryId.Type1FieldWolf, 1, 18, amountMod, BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.Type2LeafEdgeOfFaith, 30, 12, amountMod,
                BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.Type3PatchCatOfCourage, 50, 6, amountMod,
                BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.Type4DecronTroops, 90, 3, amountMod, BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.Type1FieldWolf, 120, 25, amountMod, BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.Type2LeafEdgeOfFaith, 150, 30, amountMod,
                BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.Type1FieldWolf, 181, 18, amountMod, BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.Type2LeafEdgeOfFaith, 210, 12, amountMod,
                BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.Type3PatchCatOfCourage, 240, 6, amountMod,
                BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.Type4DecronTroops, 270, 3, amountMod, BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.Type1FieldWolf, 280, 36, amountMod, BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.Type2LeafEdgeOfFaith, 310, 24, amountMod,
                BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.Type3PatchCatOfCourage, 325, 12, amountMod,
                BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.Type1FieldWolf, 361, 24, amountMod, BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.Type2LeafEdgeOfFaith, 390, 18, amountMod,
                BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.Type3PatchCatOfCourage, 420, 12, amountMod,
                BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.Type4DecronTroops, 460, 6, amountMod, BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.Type1FieldWolf, 480, 30, amountMod, BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.Type2LeafEdgeOfFaith, 500, 25, amountMod,
                BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.Type3PatchCatOfCourage, 520, 15, amountMod,
                BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.Type1FieldWolf, 541, 30, amountMod, BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.Type2LeafEdgeOfFaith, 570, 24, amountMod,
                BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.Type3PatchCatOfCourage, 600, 18, amountMod,
                BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.Type4DecronTroops, 640, 12, amountMod, BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.BossViter, 650, 1, amountMod, BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.Type2LeafEdgeOfFaith, 655, 30, amountMod,
                BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.Type3PatchCatOfCourage, 670, 24, amountMod,
                BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.Type4DecronTroops, 690, 18, amountMod, BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.Type1FieldWolf, 720, 36, amountMod, BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.Type2LeafEdgeOfFaith, 750, 30, amountMod,
                BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.Type3PatchCatOfCourage, 780, 24, amountMod,
                BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.Type4DecronTroops, 810, 18, amountMod, BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.BossViter, 820, 1, amountMod, BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.BossCommanderGeurantion, 830, 1, amountMod,
                BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.Type1FieldWolf, 840, 36, amountMod, BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.Type2LeafEdgeOfFaith, 860, 30, amountMod,
                BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.Type3PatchCatOfCourage, 880, 24, amountMod,
                BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.Type4DecronTroops, 900, 18, amountMod, BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.BossQueenKaiya, 950, 1, 1f, BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.Type1FieldWolf, 960, 5, 1f, BrainState.DefenceTownEventMove);
            this.AddSpawnEntry(NpcCustomEntryId.BossBlackEagle, 970, 1, 1f, BrainState.DefenceTownEventMove);
            base.InitMonsterSpawn(amountMod);
        }
    }
}